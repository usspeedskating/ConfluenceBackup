# ConfluenceBackup
Back up a standalone confluence server to AWS

## Overview

Creates a daily backup of the confluence server. The backup is saved to S3 glacier for 365 days (configurable). The latest backup is overwritten to S3 daily.

The glacier backup is meant to protect against accidental or malicious deletions. The S3 backup is meant for disaster recovery.


## Installation
### Prerequisites

* Confluence is installed on the server with a local PSQL Database
* Dotnet is installed on the server
* Local VM (EC2 Instance) has an IAM role with permissions to the S3 Bucket and Glacier Archive

### Steps

* PUBLISH the application to a folder using BUILD\Publish in Visual Studio
* Copy the published files to a directory on the server (such as /opt/uss/confluence-backup)
* Edit the config.json file to match the configuration of the server.
* Add the command to a nightly job (such as a cron job): `dotnet USSConfluenceBackup.dll`
  * _*Ensure the application is executed from the local working directory, else the configuration will fail to load*_
  
## Recurring Execution
Currently configured using a cron file at /etc/cron.daily/confluence-backup
```bash
#!/bin/sh
dotnet /opt/uss/confluence-backup/USSConfluenceBackup.dll /opt/uss/confluence-backup/config.json
```