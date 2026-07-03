START TRANSACTION;

ALTER TABLE "SectionField" ADD "Dimensions" text NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250120115635_AddedDimensions', '7.0.11');

COMMIT;

