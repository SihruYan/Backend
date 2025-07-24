-- 啟用 UUID 產生 extension
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- 建立資料表
CREATE TABLE IF NOT EXISTS public.form_responses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    answers JSONB NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT now()
);
-- 建立精簡版 admin_users 資料表

CREATE TABLE IF NOT EXISTS public.admin_users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL
    );

-- 建立索引以提升查詢效能
CREATE INDEX IF NOT EXISTS idx_admin_users_username ON public.admin_users(username);
CREATE INDEX IF NOT EXISTS idx_admin_users_email ON public.admin_users(email);
