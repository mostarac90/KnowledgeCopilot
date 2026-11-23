CREATE TABLE IF NOT EXISTS documents
(
    id UUID PRIMARY KEY,
    file_name TEXT NOT NULL,
    content_type TEXT NOT NULL,
    size_bytes BIGINT NOT NULL,
    original_pdf BYTEA NOT NULL,
    page_count INTEGER NOT NULL,
    uploaded_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS document_pages
(
    document_id UUID NOT NULL
        REFERENCES documents(id) ON DELETE CASCADE,

    page_number INTEGER NOT NULL,
    extracted_text TEXT NOT NULL,

    PRIMARY KEY (document_id, page_number)
);