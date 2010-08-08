SouthWind Example Database for Oracle

Many of the example found on the web for ASP.NET use the Northwind example database that is provided with many of the falvors of MS SQL SERVER.  That's dandy, if you have MS SQL, but what about Oracle.  Most of the examples work practically unchange in Oracle, so I thought it would be convienient if there was a Northwind equivilent for Oracle.

Enter Southwind. Southwind is a port of the Northwind example database into Oracle and thus allows the Oracle user the benefits of the zillions of example available on the web.  The attached ZIP file contains the necessary scripts to create the Southwind database.

READ CAREFULLY
Extact the script to a working direction.  They are desinged to be run from the SQL*PLUS command line.  The main routine SWInst.sql load and executes the remaining scripts to complete the installation.

NOTE: SWInst.sql does not create the Southwind Schema.  The included script CreateUser.sql can be used to setup the Schema ready for installation.  There is no real need to keep the Southwind name if you want to change.  To create the schema, login as a DBA and execute the script.

NOTE: The first line in SWInst.sql, DeleteTables.sql, is commented out on purpose.  When first installing, it shouldn't be necessary.  However, if you need to restart the installation, then uncomment this line.  WARNING -- THIS WILL DELETE PRETTY MUCH EVERYTHING IN THE SCHEMA WHERE IT IS RUN -- WARNING.  If you happen to be connected as some other user when you execute this -- well, just make sure you have backups.

To Install:
 Login to Oracle using sqlplus:
   SQLPLUS SouthWind/SouthWind@XE  (where XE your oracle home)
   SQL> @SWInst.sql

That should do it.

Limitations
I did not port over the LOB's, such as photos and Notes.  