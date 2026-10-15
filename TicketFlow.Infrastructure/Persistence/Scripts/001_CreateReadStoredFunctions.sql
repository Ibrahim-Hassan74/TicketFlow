-- ===================================================================================
-- TicketFlow: PostgreSQL Read Stored Functions (Dapper Read Optimization)
-- Idempotent script for creating/updating stored functions used by Dapper queries.
-- ===================================================================================

-- 1. Dashboard Stats Function
CREATE OR REPLACE FUNCTION sp_get_dashboard_stats()
RETURNS TABLE (
    "TotalTickets" INT,
    "OpenTickets" INT,
    "InProgressTickets" INT,
    "ResolvedTickets" INT,
    "ClosedTickets" INT,
    "CriticalOpenTickets" INT,
    "AvgResolutionTimeHours" NUMERIC
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        COUNT(*)::INT AS "TotalTickets",
        COUNT(*) FILTER (WHERE "Status" = 0)::INT AS "OpenTickets",
        COUNT(*) FILTER (WHERE "Status" = 1)::INT AS "InProgressTickets",
        COUNT(*) FILTER (WHERE "Status" = 2)::INT AS "ResolvedTickets",
        COUNT(*) FILTER (WHERE "Status" = 3)::INT AS "ClosedTickets",
        COUNT(*) FILTER (WHERE "Status" = 0 AND "Priority" = 3)::INT AS "CriticalOpenTickets",
        COALESCE(
            ROUND(AVG(EXTRACT(EPOCH FROM ("ResolvedAt" - "CreatedAt")) / 3600.0) 
            FILTER (WHERE "Status" IN (2, 3) AND "ResolvedAt" IS NOT NULL)::NUMERIC, 2), 
            0.0
        ) AS "AvgResolutionTimeHours"
    FROM "Tickets";
END;
$$ LANGUAGE plpgsql;

-- 2. Agent Workload Function
CREATE OR REPLACE FUNCTION sp_get_agent_workload()
RETURNS TABLE (
    "AgentId" UUID,
    "AgentName" TEXT,
    "OpenTickets" INT,
    "InProgressTickets" INT,
    "TotalTimeMinutes" INT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        u."Id" AS "AgentId",
        u."DisplayName"::TEXT AS "AgentName",
        COUNT(t."Id") FILTER (WHERE t."Status" = 0)::INT AS "OpenTickets",
        COUNT(t."Id") FILTER (WHERE t."Status" = 1)::INT AS "InProgressTickets",
        COALESCE(SUM(te."DurationMinutes"), 0)::INT AS "TotalTimeMinutes"
    FROM "AspNetUsers" u
    INNER JOIN "Tickets" t ON t."AssignedAgentId" = u."Id"
    LEFT JOIN "TimeEntries" te ON te."AgentId" = u."Id"
    WHERE u."IsDeleted" = FALSE
    GROUP BY u."Id", u."DisplayName";
END;
$$ LANGUAGE plpgsql;

-- 3. Ticket Trends Function
CREATE OR REPLACE FUNCTION sp_get_ticket_trends(p_days INT)
RETURNS TABLE (
    "Date" DATE,
    "OpenCount" INT,
    "ClosedCount" INT
) AS $$
BEGIN
    RETURN QUERY
    WITH date_series AS (
        SELECT generate_series(
            CURRENT_DATE - (p_days - 1) * INTERVAL '1 day',
            CURRENT_DATE,
            INTERVAL '1 day'
        )::DATE AS d
    )
    SELECT 
        ds.d AS "Date",
        COUNT(t."Id") FILTER (WHERE t."CreatedAt"::DATE = ds.d)::INT AS "OpenCount",
        COUNT(t."Id") FILTER (WHERE t."ClosedAt" IS NOT NULL AND t."ClosedAt"::DATE = ds.d)::INT AS "ClosedCount"
    FROM date_series ds
    LEFT JOIN "Tickets" t ON t."CreatedAt"::DATE = ds.d OR (t."ClosedAt" IS NOT NULL AND t."ClosedAt"::DATE = ds.d)
    GROUP BY ds.d
    ORDER BY ds.d ASC;
END;
$$ LANGUAGE plpgsql;

-- 4. Filtered & Paginated Tickets Function
CREATE OR REPLACE FUNCTION sp_get_tickets(
    p_role VARCHAR,
    p_user_id UUID,
    p_status INT,
    p_priority INT,
    p_search VARCHAR,
    p_page INT,
    p_page_size INT,
    p_sort_by VARCHAR,
    p_sort_order VARCHAR
)
RETURNS TABLE (
    "Id" UUID,
    "Title" TEXT,
    "Description" TEXT,
    "Status" INT,
    "Priority" INT,
    "CustomerId" UUID,
    "CustomerName" TEXT,
    "AssignedAgentId" UUID,
    "AssignedAgentName" TEXT,
    "CreatedAt" TIMESTAMPTZ,
    "UpdatedAt" TIMESTAMPTZ,
    "ResolvedAt" TIMESTAMPTZ,
    "ClosedAt" TIMESTAMPTZ,
    "TotalCount" BIGINT
) AS $$
DECLARE
    v_offset INT := (p_page - 1) * p_page_size;
BEGIN
    RETURN QUERY
    SELECT 
        t."Id",
        t."Title"::TEXT,
        t."Description"::TEXT,
        t."Status",
        t."Priority",
        t."CustomerId",
        c."DisplayName"::TEXT AS "CustomerName",
        t."AssignedAgentId",
        a."DisplayName"::TEXT AS "AssignedAgentName",
        t."CreatedAt",
        t."UpdatedAt",
        t."ResolvedAt",
        t."ClosedAt",
        COUNT(*) OVER() AS "TotalCount"
    FROM "Tickets" t
    INNER JOIN "AspNetUsers" c ON c."Id" = t."CustomerId"
    LEFT JOIN "AspNetUsers" a ON a."Id" = t."AssignedAgentId"
    WHERE 
        (p_role = 'Admin' OR 
         (p_role = 'SupportAgent' AND t."AssignedAgentId" = p_user_id) OR 
         (p_role = 'Customer' AND t."CustomerId" = p_user_id))
        AND (p_status IS NULL OR t."Status" = p_status)
        AND (p_priority IS NULL OR t."Priority" = p_priority)
        AND (p_search IS NULL OR p_search = '' OR t."Title" ILIKE '%' || p_search || '%')
    ORDER BY
        CASE WHEN p_sort_by = 'Priority' AND p_sort_order = 'Ascending' THEN t."Priority" END ASC,
        CASE WHEN p_sort_by = 'Priority' AND p_sort_order = 'Descending' THEN t."Priority" END DESC,
        CASE WHEN p_sort_by = 'Status' AND p_sort_order = 'Ascending' THEN t."Status" END ASC,
        CASE WHEN p_sort_by = 'Status' AND p_sort_order = 'Descending' THEN t."Status" END DESC,
        CASE WHEN p_sort_by = 'UpdatedAt' AND p_sort_order = 'Ascending' THEN t."UpdatedAt" END ASC,
        CASE WHEN p_sort_by = 'UpdatedAt' AND p_sort_order = 'Descending' THEN t."UpdatedAt" END DESC,
        CASE WHEN (p_sort_by IS NULL OR p_sort_by = 'CreatedAt') AND p_sort_order = 'Ascending' THEN t."CreatedAt" END ASC,
        CASE WHEN (p_sort_by IS NULL OR p_sort_by = 'CreatedAt') AND (p_sort_order IS NULL OR p_sort_order = 'Descending') THEN t."CreatedAt" END DESC
    LIMIT p_page_size OFFSET v_offset;
END;
$$ LANGUAGE plpgsql;

-- 5. Paginated Users Function
CREATE OR REPLACE FUNCTION sp_get_users(
    p_role VARCHAR,
    p_search VARCHAR,
    p_page INT,
    p_page_size INT
)
RETURNS TABLE (
    "Id" UUID,
    "Email" TEXT,
    "DisplayName" TEXT,
    "Role" TEXT,
    "IsActive" BOOLEAN,
    "CreatedAt" TIMESTAMPTZ,
    "TotalCount" BIGINT
) AS $$
DECLARE
    v_offset INT := (p_page - 1) * p_page_size;
BEGIN
    RETURN QUERY
    SELECT 
        u."Id",
        COALESCE(u."Email", '')::TEXT AS "Email",
        u."DisplayName"::TEXT AS "DisplayName",
        COALESCE(r."Name", '')::TEXT AS "Role",
        (NOT u."IsDeleted") AS "IsActive",
        u."CreatedAt",
        COUNT(*) OVER() AS "TotalCount"
    FROM "AspNetUsers" u
    LEFT JOIN "AspNetUserRoles" ur ON ur."UserId" = u."Id"
    LEFT JOIN "AspNetRoles" r ON r."Id" = ur."RoleId"
    WHERE 
        (p_role IS NULL OR p_role = '' OR r."Name" = p_role)
        AND (p_search IS NULL OR p_search = '' OR 
             u."DisplayName" ILIKE '%' || p_search || '%' OR 
             u."Email" ILIKE '%' || p_search || '%')
    ORDER BY u."DisplayName" ASC
    LIMIT p_page_size OFFSET v_offset;
END;
$$ LANGUAGE plpgsql;

-- 6. Paginated Agents Function
CREATE OR REPLACE FUNCTION sp_get_agents(
    p_page INT,
    p_page_size INT
)
RETURNS TABLE (
    "Id" UUID,
    "Email" TEXT,
    "DisplayName" TEXT,
    "Role" TEXT,
    "IsActive" BOOLEAN,
    "CreatedAt" TIMESTAMPTZ,
    "TotalCount" BIGINT
) AS $$
DECLARE
    v_offset INT := (p_page - 1) * p_page_size;
BEGIN
    RETURN QUERY
    SELECT 
        u."Id",
        COALESCE(u."Email", '')::TEXT AS "Email",
        u."DisplayName"::TEXT AS "DisplayName",
        'SupportAgent'::TEXT AS "Role",
        (NOT u."IsDeleted") AS "IsActive",
        u."CreatedAt",
        COUNT(*) OVER() AS "TotalCount"
    FROM "AspNetUsers" u
    INNER JOIN "AspNetUserRoles" ur ON ur."UserId" = u."Id"
    INNER JOIN "AspNetRoles" r ON r."Id" = ur."RoleId" AND r."Name" = 'SupportAgent'
    WHERE u."IsDeleted" = FALSE
    ORDER BY u."CreatedAt" ASC
    LIMIT p_page_size OFFSET v_offset;
END;
$$ LANGUAGE plpgsql;

-- 7. Ticket Comments Function
CREATE OR REPLACE FUNCTION sp_get_ticket_comments(p_ticket_id UUID)
RETURNS TABLE (
    "Id" UUID,
    "TicketId" UUID,
    "UserId" UUID,
    "UserName" TEXT,
    "Content" TEXT,
    "CreatedAt" TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        c."Id",
        c."TicketId",
        c."UserId",
        COALESCE(u."DisplayName", 'Unknown')::TEXT AS "UserName",
        c."Content"::TEXT,
        c."CreatedAt"
    FROM "TicketComments" c
    LEFT JOIN "AspNetUsers" u ON u."Id" = c."UserId"
    WHERE c."TicketId" = p_ticket_id
    ORDER BY c."CreatedAt" ASC;
END;
$$ LANGUAGE plpgsql;

-- 8. Ticket Activities Function
CREATE OR REPLACE FUNCTION sp_get_ticket_activities(p_ticket_id UUID)
RETURNS TABLE (
    "Id" UUID,
    "TicketId" UUID,
    "UserId" UUID,
    "UserName" TEXT,
    "Type" TEXT,
    "Description" TEXT,
    "OldValue" TEXT,
    "NewValue" TEXT,
    "CreatedAt" TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        a."Id",
        a."TicketId",
        a."UserId",
        COALESCE(u."DisplayName", 'Unknown')::TEXT AS "UserName",
        CASE a."Type"
            WHEN 0 THEN 'Created'
            WHEN 1 THEN 'StatusChanged'
            WHEN 2 THEN 'PriorityChanged'
            WHEN 3 THEN 'AgentAssigned'
            WHEN 4 THEN 'CommentAdded'
            WHEN 5 THEN 'TimeEntryAdded'
            ELSE 'Created'
        END::TEXT AS "Type",
        a."Description"::TEXT,
        a."OldValue"::TEXT,
        a."NewValue"::TEXT,
        a."CreatedAt"
    FROM "TicketActivities" a
    LEFT JOIN "AspNetUsers" u ON u."Id" = a."UserId"
    WHERE a."TicketId" = p_ticket_id
    ORDER BY a."CreatedAt" ASC;
END;
$$ LANGUAGE plpgsql;

-- 9. Ticket Time Entries Function
CREATE OR REPLACE FUNCTION sp_get_ticket_time_entries(p_ticket_id UUID)
RETURNS TABLE (
    "Id" UUID,
    "TicketId" UUID,
    "AgentId" UUID,
    "AgentName" TEXT,
    "WorkDate" DATE,
    "DurationMinutes" INT,
    "Description" TEXT,
    "CreatedAt" TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        te."Id",
        te."TicketId",
        te."AgentId",
        COALESCE(u."DisplayName", 'Unknown')::TEXT AS "AgentName",
        te."WorkDate",
        te."DurationMinutes",
        te."Description"::TEXT,
        te."CreatedAt"
    FROM "TimeEntries" te
    LEFT JOIN "AspNetUsers" u ON u."Id" = te."AgentId"
    WHERE te."TicketId" = p_ticket_id
    ORDER BY te."WorkDate" ASC, te."CreatedAt" ASC;
END;
$$ LANGUAGE plpgsql;
