/*

Database name - pan_verification2



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
    'http://localhost:3001',
    '/api/v1/pan/pan-adv-v2',
    'MOCK_TOKEN',
    1
),
(
    'sprintverify',
    'http://localhost:3002',
    '/api/v1/sprintverify',
    'MOCK_TOKEN',
    2
);

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
    p_createdat TIMESTAMPTZ   -- ✅ FIX HERE
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
    p_status INTEGER,
    p_data_code VARCHAR,
    p_response_timestamp TIMESTAMPTZ,
    p_response_time_ms INTEGER,
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
        status,
        data_code,
        response_timestamp,
        response_time_ms,
        encrypted_raw_response_json,
        created_at
    )
    VALUES (
        p_correlation_id,
        p_pan_verification_id,
        p_request_id,
        p_status,
        p_data_code,
        p_response_timestamp,
        p_response_time_ms,
        p_encrypted_raw_response_json,
        p_created_at
    );
END;
$$;
//--------------------------------


DROP TABLE panresponsesjson;
DROP TABLE panverifications;
DROP TABLE providerpanmaster;


DELETE FROM panverifications;


SELECT * FROM providerpanmaster;
SELECT * FROM panresponsesjson;
SELECT * FROM panverifications;