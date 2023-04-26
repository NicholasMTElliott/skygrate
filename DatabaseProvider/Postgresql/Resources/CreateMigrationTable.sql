BEGIN;

CREATE TABLE IF NOT EXISTS _sg_migration_tracking (
	Id varchar(8) NOT NULL PRIMARY KEY,
	Timestamp varchar(14) NOT NULL,
	Name text NOT NULL,
	Checksum varchar(32) NOT NULL,
	
	PreviousMigrationId varchar(8) DEFAULT NULL,
	Applied timestamp with time zone NOT NULL DEFAULT NOW (),	
	RollingChecksum varchar(32) NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_sg_migration_tracking_ordered ON _sg_migration_tracking (Timestamp);

COMMIT;