/*


CREATE TABLE providerpanmaster (
    id BIGSERIAL PRIMARY KEY,
    provider_name VARCHAR(50) NOT NULL UNIQUE,
    provider_baseurl VARCHAR(500) NOT NULL,
    provider_endpoint VARCHAR(100) NOT NULL,
    encrypted_api_key VARCHAR(500) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    priority INTEGER NOT NULL,
    timeout_ms INTEGER NOT NULL DEFAULT 5000,
    retry_count INTEGER NOT NULL DEFAULT 1,
    createdat TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updatedat TIMESTAMPTZ NULL
);


CREATE TABLE panverifications (
    id UUID PRIMARY KEY,
    correlationid VARCHAR(100) NOT NULL,
    masterid BIGINT NOT NULL,
    panhash VARCHAR(64) NOT NULL UNIQUE,
    encryptedpan VARCHAR(500) NOT NULL,
    providerrequestid VARCHAR(100),
    panstatus VARCHAR(10),
    panlookupstatus VARCHAR(20) NOT NULL,
    encryptedfullname VARCHAR(500),
    pancardtype VARCHAR(20) NOT NULL,
    ispanaadhaarliked BOOLEAN,
    callerip VARCHAR(45),
    createdat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updatedat TIMESTAMP NULL,
    softdeletedat TIMESTAMP NULL,
    softdeletedby INTEGER NULL,

    CONSTRAINT fk_pan_master
        FOREIGN KEY (masterid)
        REFERENCES providerpanmaster(id)
        ON DELETE RESTRICT
);



CREATE TABLE panresponsesjson (
    id BIGSERIAL PRIMARY KEY,
    correlation_id VARCHAR(100) NOT NULL,
    pan_verification_id UUID NOT NULL,
    request_id VARCHAR(100),
    status INTEGER,
    data_code VARCHAR(10),
    response_timestamp TIMESTAMPTZ,
    response_time_ms INTEGER,
    encrypted_raw_response_json TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_pan_verification
        FOREIGN KEY (pan_verification_id)
        REFERENCES panverifications(id)
        ON DELETE CASCADE
);


CREATE EXTENSION IF NOT EXISTS "uuid-ossp";



INSERT INTO providerpanmaster
(
    provider_name,
    provider_baseurl,
    provider_endpoint,
    encrypted_api_key,
    priority
)
VALUES
(
    'surepass',
    'https://kyc-api.surepass.io',
    '/api/v1/pan/pan-adv-v2',
    'eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJmcmVzaCI6ZmFsc2UsImlhdCI6MTY1ODkyMjkwNCwianRpIjoiNGFjZTdkZGItMDAwNi00NmNmLWFiYWYtNTc4OTI0YTg3ZjI3IiwidHlwZSI6ImFjY2VzcyIsImlkZW50aXR5IjoiZGV2LnBheXBvaW50aW5kaWFAc3VyZXBhc3MuaW8iLCJuYmYiOjE2NTg5MjI5MDQsImV4cCI6MTk3NDI4MjkwNCwidXNlcl9jbGFpbXMiOnsic2NvcGVzIjpbIndhbGxldCJdfX0.RBdnMnu1CIRb2flUzHNkzZYvOI-wN1CabDp_ZR4fXOQ',
    2
),
(
    'sprintverify',
    'http://localhost:3002',
    '/api/v1/sprintverify',
    'MOCK_TOKEN',
    1
);


UPDATE providerpanmaster
SET 
    priority = 2,
    encrypted_api_key = 'eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJmcmVzaCI6ZmFsc2UsImlhdCI6MTY1ODkyMjkwNCwianRpIjoiNGFjZTdkZGItMDAwNi00NmNmLWFiYWYtNTc4OTI0YTg3ZjI3IiwidHlwZSI6ImFjY2VzcyIsImlkZW50aXR5IjoiZGV2LnBheXBvaW50aW5kaWFAc3VyZXBhc3MuaW8iLCJuYmYiOjE2NTg5MjI5MDQsImV4cCI6MTk3NDI4MjkwNCwidXNlcl9jbGFpbXMiOnsic2NvcGVzIjpbIndhbGxldCJdfX0.RBdnMnu1CIRb2flUzHNkzZYvOI-wN1CabDp_ZR4fXOQ'
WHERE provider_name = 'surepass';

UPDATE providerpanmaster
SET 
    priority = 1
WHERE provider_name = 'sprintverify';
//--------------------------------------

DROP PROCEDURE IF EXISTS insert_pan_verification;

CREATE OR REPLACE PROCEDURE insert_pan_verification(
    p_id UUID,
    p_correlationid VARCHAR,
    p_masterid BIGINT,
    p_providerrequestid VARCHAR,
    p_panhash VARCHAR,
    p_encryptedpan VARCHAR,
    p_panstatus VARCHAR,
    p_panlookupstatus VARCHAR,
    p_encryptedfullname VARCHAR,
    p_pancardtype VARCHAR,
    p_ispanaadhaarliked BOOLEAN,
    p_callerip VARCHAR,
    p_createdat TIMESTAMPTZ   
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO panverifications (
        id,
        correlationid,
        masterid,
        providerrequestid,
        panhash,
        encryptedpan,
        panstatus,
        panlookupstatus,
        encryptedfullname,
        pancardtype,
        ispanaadhaarliked,
        callerip,
        createdat
    )
    VALUES (
        p_id,
        p_correlationid,
        p_masterid,
        p_providerrequestid,
        p_panhash,
        p_encryptedpan,
        p_panstatus,
        p_panlookupstatus,
        p_encryptedfullname,
        p_pancardtype,
        p_ispanaadhaarliked,
        p_callerip,
        p_createdat
    );
END;
$$;



DROP PROCEDURE IF EXISTS insert_pan_response;
CREATE OR REPLACE PROCEDURE insert_pan_response(
    p_correlation_id VARCHAR,
    p_pan_verification_id UUID,
    p_request_id VARCHAR,
    p_encrypted_raw_response_json TEXT,
    p_created_at TIMESTAMPTZ
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO panresponsesjson (
        correlation_id,
        pan_verification_id,
        request_id,
        encrypted_raw_response_json,
        created_at
    )
    VALUES (
        p_correlation_id,
        p_pan_verification_id,
        p_request_id,
        p_encrypted_raw_response_json,
        p_created_at
    );
END;
$$;

DROP FUNCTION IF EXISTS get_pan_with_provider(TEXT);

CREATE OR REPLACE FUNCTION get_provider_name_by_hash(p_hash TEXT)
RETURNS TEXT AS $$
BEGIN
    RETURN (
        SELECT m.provider_name
        FROM panverifications p
        JOIN providerpanmaster m ON p.masterid = m.id
        WHERE p.panhash = p_hash
        LIMIT 1
    );
END;
$$ LANGUAGE plpgsql;


DROP PROCEDURE IF EXISTS insert_pan_verification(UUID, VARCHAR, BIGINT, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, BOOLEAN, VARCHAR, TIMESTAMPTZ);


CREATE OR REPLACE FUNCTION insert_pan_verification(
    p_id UUID,
    p_correlationid VARCHAR,
    p_masterid BIGINT,
    p_providerrequestid VARCHAR,
    p_panhash VARCHAR,
    p_encryptedpan VARCHAR,
    p_panstatus VARCHAR,
    p_panlookupstatus VARCHAR,
    p_encryptedfullname VARCHAR,
    p_pancardtype VARCHAR,
    p_ispanaadhaarliked BOOLEAN,
    p_callerip VARCHAR,
    p_createdat TIMESTAMPTZ
)
RETURNS UUID
LANGUAGE plpgsql
AS $$
DECLARE
    v_id UUID;
BEGIN
    INSERT INTO panverifications (
        id, correlationid, masterid, providerrequestid,
        panhash, encryptedpan, panstatus, panlookupstatus,
        encryptedfullname, pancardtype, ispanaadhaarliked,
        callerip, createdat
    )
    VALUES (
        p_id, p_correlationid, p_masterid, p_providerrequestid,
        p_panhash, p_encryptedpan, p_panstatus, p_panlookupstatus,
        p_encryptedfullname, p_pancardtype, p_ispanaadhaarliked,
        p_callerip, p_createdat
    )
    ON CONFLICT (panhash) DO NOTHING;

    SELECT id INTO v_id FROM panverifications WHERE panhash = p_panhash;
    RETURN v_id;
END;
$$;

SELECT 
        id,
        provider_name       AS ProviderName,
        provider_baseurl    AS BaseUrl,
        provider_endpoint   AS Endpoint,
        encrypted_api_key   AS ApiKey,
        priority            AS Priority,
        is_active           AS IsActive
    FROM providerpanmaster
    WHERE is_active = true



	CREATE OR REPLACE FUNCTION get_active_providers()
RETURNS TABLE (
    id                BIGINT,
    providername VARCHAR,
    baseurl VARCHAR,
    endpoint VARCHAR,
    apikey VARCHAR,
    priority INTEGER,
    isactive BOOLEAN
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        pm.id,
        pm.provider_name AS providername,
        pm.provider_baseurl AS baseurl,
        pm.provider_endpoint AS endpoint,
        pm.encrypted_api_key AS apikey,
        pm.priority AS priority,
        pm.is_active AS isactive
    FROM providerpanmaster pm
    WHERE pm.is_active = true
    ORDER BY pm.priority ASC;
END;
$$;
//--------------------------------


-- DROP TABLE panresponsesjson;
-- DROP TABLE panverifications;
-- DROP TABLE providerpanmaster;

-- TRUNCATE TABLE panverifications CASCADE;

SELECT * FROM providerpanmaster;
SELECT * FROM panresponsesjson;
SELECT * FROM panverifications;nverifications;