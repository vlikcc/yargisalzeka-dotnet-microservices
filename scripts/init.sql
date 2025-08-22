-- Production Database Initialization Script
-- This script creates all necessary databases for the microservices

-- Create databases
CREATE DATABASE IF NOT EXISTS "IdentityDb";
CREATE DATABASE IF NOT EXISTS "SubscriptionDb";
CREATE DATABASE IF NOT EXISTS "DocumentDb";
CREATE DATABASE IF NOT EXISTS "yargitay_kararlari";
CREATE DATABASE IF NOT EXISTS "AIDb";

-- Grant permissions to postgres user
GRANT ALL PRIVILEGES ON DATABASE "IdentityDb" TO postgres;
GRANT ALL PRIVILEGES ON DATABASE "SubscriptionDb" TO postgres;
GRANT ALL PRIVILEGES ON DATABASE "DocumentDb" TO postgres;
GRANT ALL PRIVILEGES ON DATABASE "yargitay_kararlari" TO postgres;
GRANT ALL PRIVILEGES ON DATABASE "AIDb" TO postgres;

-- Create extensions in each database (run these separately for each DB)
-- For IdentityDb:
-- \c IdentityDb;
-- CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
-- CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- For yargitay_kararlari (legal decisions database):
-- \c yargitay_kararlari;
-- CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
-- CREATE EXTENSION IF NOT EXISTS "pg_trgm"; -- For text search
-- CREATE EXTENSION IF NOT EXISTS "btree_gin"; -- For indexing

-- Create indexes and optimizations for yargitay_kararlari
-- \c yargitay_kararlari;
-- CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_decisions_title ON decisions USING gin(to_tsvector('turkish', title));
-- CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_decisions_content ON decisions USING gin(to_tsvector('turkish', content));
-- CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_decisions_court ON decisions(court);
-- CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_decisions_decision_date ON decisions(decision_date DESC);

-- Set timezone
SET timezone = 'Europe/Istanbul';

-- Create a monitoring role (optional)
-- CREATE ROLE monitoring_user WITH LOGIN PASSWORD 'monitoring_password';
-- GRANT CONNECT ON DATABASE "IdentityDb" TO monitoring_user;
-- GRANT CONNECT ON DATABASE "SubscriptionDb" TO monitoring_user;
-- GRANT CONNECT ON DATABASE "DocumentDb" TO monitoring_user;
-- GRANT CONNECT ON DATABASE "yargitay_kararlari" TO monitoring_user;
-- GRANT CONNECT ON DATABASE "AIDb" TO monitoring_user;

-- Create backup user (optional)
-- CREATE ROLE backup_user WITH LOGIN PASSWORD 'backup_password' SUPERUSER;
-- ALTER ROLE backup_user CREATEDB;
-- ALTER ROLE backup_user CREATEROLE;
