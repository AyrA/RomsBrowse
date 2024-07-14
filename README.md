# RomsBrowse

ROM browser with built-in emulator and server side game saves

## Running the Application

The application can run standalone, as IIS application, or as a service.

### Standalone

To run the application in standalone mode,
run it from the command line and supply the `--urls` argument to tell it where to listen on.

Example: `--urls http://localhost:54321`

Then simply point your webbrowser at this address

### IIS

When using `dotnet publish`, it creates the necessary `web.config`
to make IIS launch the application automatically.
Simply create a new website in IIS, then dump all application files into the web directory.

### Service

This is the recommended way to run the application.
This way it runs even when nobody is logged into the device,
and it can be used directly, or in combination with any webserver that supports reverse proxy operations,
not just IIS.

To install the application as a service, do the following:

1. Put the application into an empty directory of your choice
2. Run the command `sc create RomsBrowse binPath= "C:\Path\To\RomsBrowse.Web.exe --urls http://localhost:54321"`
3. Run `services.msc` and double click on the RomsBrowse service
4. Change the startup type to automatic
5. In the "Log on" tab, select "This account", then type `NT Service\RomsBrowse`, and erase the password fields.
6. Click "OK"
7. Open the properties of the wwwroot directory, and grant write access to `NT Service\RomsBrowse`
8. (If using SQL server) Connect to your SQL server and create a database named "roms"
9. (If using SQL server) Grant `db_datareader`, `db_datawriter` and `ddl_admin` rights to `NT Service\RomsBrowse`.
10. Start the service

## Initial Configuration

Until the database settings have been configured,
all requests will be redirected to the setup page.
Fill in the details relevant for your SQL setup, then save the settings.

RomsBrowse supports SQLite and SQL server

## Initial Account

The first account you create will be the admin account.
To ensure nobody else can do this, you must copy the value of the "AdminToken"
from the "Settings" SQL table.

The registration form provides a field to paste this value into.

## Configuration

When logged in as administrator,
you can use the admin drop down menu in the top navbar to open the application settings,
there you can configure all aspects of the application.

## ROMs Directory

The ROM file directory needs to have a certain layout to be usable.
The application contains a help for how to set up the directory structure in the admin menu
when you're logged in as administrator

## Server Side Save States

When signed in, save states are saved on the server,
and restored automatically when the emulator is run the next time.

A state is automatically uploaded whenever
the "Save State" button in the emulator menu bar is pressed.

## Server Side SRAM

Game memory that is used by the game for the regular save function
is uploaded and restored from the server in a similar fashion to save states.

The application detects automatically when the SRAM changes,
and then uploads it.

## Pending Tasks

