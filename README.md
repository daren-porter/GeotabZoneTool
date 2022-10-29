# GeotabZoneTool
A simple console application that polls the [MyGeotab API](https://geotab.github.io/sdk/software/api/reference/) and reports all Zones that overlap with one another.  Results are written in JSON to the application's working directory.

This utility is meant to be a fast, easy-to-use aid for Geotab implementers to quickly identify zones that overlap with one another.

## How To Use

1. Clone the repository
2. Modify the string constants at the top of `Program.cs` with credentials for the Geotab database containing zones you would like to compare.  You must specify the database and *either* a username and password *or* a valid session ID at minimum.  If the specified user exists in multiple databases, then you should also supply the server path (i.e. my.geotab.com).
3. Run the program.  A summary of the results will be displayed in the console, and a more detailed breakdown will be written to a JSON file in the application's working directory.

## Why would I need this?

At time of writing, there is a known bug in the Geotab Platform's Rules engine that can cause triggering geofence crossing events to be missed for zones that overlap.  This occurs whenever two zones that touch or overlap in any way share a Rule and that Rule is triggered by crossing the zone's barrier.  In these situations, the rules engine's behavior becomes unpredictable, and it can fail to report geofence crossing events entirely for either zone.

Geotab does not have any built-in reporting that lists zones which overlap, making it cumbersome to identify them, especially for larger databases that can contain thousands of zones.  This utility aims to simplify that process.
