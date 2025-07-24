-- 啟用 UUID 產生 extension
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- 建立資料表
CREATE TABLE IF NOT EXISTS public.form_responses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    answers JSONB NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT now()
);