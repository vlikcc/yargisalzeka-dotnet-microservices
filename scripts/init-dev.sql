-- Development Database Initialization Script
-- This script creates development databases that won't interfere with production data

-- Create development databases (different names to protect production data)
CREATE DATABASE IF NOT EXISTS "IdentityDb_dev";
CREATE DATABASE IF NOT EXISTS "SubscriptionDb_dev";
CREATE DATABASE IF NOT EXISTS "DocumentDb_dev";
CREATE DATABASE IF NOT EXISTS "yargitay_kararlari_dev";
CREATE DATABASE IF NOT EXISTS "AIDb_dev";

-- Grant permissions to postgres user for development databases
GRANT ALL PRIVILEGES ON DATABASE "IdentityDb_dev" TO postgres;
GRANT ALL PRIVILEGES ON DATABASE "SubscriptionDb_dev" TO postgres;
GRANT ALL PRIVILEGES ON DATABASE "DocumentDb_dev" TO postgres;
GRANT ALL PRIVILEGES ON DATABASE "yargitay_kararlari_dev" TO postgres;
GRANT ALL PRIVILEGES ON DATABASE "AIDb_dev" TO postgres;

-- Set timezone for development
SET timezone = 'Europe/Istanbul';

-- Note: Extensions will be created automatically by Entity Framework migrations
-- when services start for the first time
