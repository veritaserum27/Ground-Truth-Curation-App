-- Create the support_tickets table based on the CSV structure
-- This script should be run after the database is created

CREATE TABLE support_tickets (
    ticket_id BIGINT PRIMARY KEY,
    day_of_week NVARCHAR(10) NOT NULL,
    day_of_week_num INT NOT NULL,
    company_id INT NOT NULL,
    company_size NVARCHAR(20) NOT NULL,
    company_size_cat INT NOT NULL,
    industry NVARCHAR(50) NOT NULL,
    industry_cat INT NOT NULL,
    customer_tier NVARCHAR(20) NOT NULL,
    customer_tier_cat INT NOT NULL,
    org_users INT NOT NULL,
    region NVARCHAR(10) NOT NULL,
    region_cat INT NOT NULL,
    past_30d_tickets INT NOT NULL,
    past_90d_incidents INT NOT NULL,
    product_area NVARCHAR(50) NOT NULL,
    product_area_cat INT NOT NULL,
    booking_channel NVARCHAR(20) NOT NULL,
    booking_channel_cat INT NOT NULL,
    reported_by_role NVARCHAR(50) NOT NULL,
    reported_by_role_cat INT NOT NULL,
    customers_affected INT NOT NULL,
    error_rate_pct DECIMAL(15,9) NOT NULL,
    downtime_min INT NOT NULL,
    payment_impact_flag BIT NOT NULL,
    security_incident_flag BIT NOT NULL,
    data_loss_flag BIT NOT NULL,
    has_runbook BIT NOT NULL,
    customer_sentiment NVARCHAR(20) NULL,
    customer_sentiment_cat INT NOT NULL,
    description_length INT NOT NULL,
    priority NVARCHAR(20) NOT NULL,
    priority_cat INT NOT NULL
);

-- Create indexes for commonly queried columns
CREATE INDEX IX_support_tickets_company_id ON support_tickets(company_id);
CREATE INDEX IX_support_tickets_priority ON support_tickets(priority);
CREATE INDEX IX_support_tickets_customer_tier ON support_tickets(customer_tier);
CREATE INDEX IX_support_tickets_product_area ON support_tickets(product_area);
CREATE INDEX IX_support_tickets_region ON support_tickets(region);
CREATE INDEX IX_support_tickets_day_of_week ON support_tickets(day_of_week);
