# Official Lytics Connector for Sitecore
This is a module that connects to Lytics to provide a hook for real time Lytics segment personalization using the rules engine in Sitecore.

# Upgrading from Legacy Version

## Overwriting Existing Items:
During installation, Sitecore may prompt you to overwrite the existing Lytics rule. Please proceed with the overwrite.

## Configuration:
After installation, edit the Lytics.config file to include your access token.

## Publishing:
Publish the Lytics segment items.

## Cleanup:
Unpublish and delete any old, duplicated items under /sitecore/system/Settings/Rules/Definitions/Lytics.

## Verification:
Once all the steps above are completed, you should be able to see the relevant logs.

## Important:
Please ensure you take backups of the Sitecore items, DLLs, and configuration files before proceeding with the installation.
