Declare
    viewName sys.user_views.VIEW_NAME%TYPE;
    tableName sys.user_tables.TABLE_NAME%TYPE;
    sqlcmd varchar2(100);
   -- objectType sys.user_objects.OBJECT_TYPE%TYPE;
    cursor c1 IS
        select view_name from sys.user_views;  
        
    cursor c2 IS   
        With dropTable as (
            select table_name ,
                Decode(Upper(table_name),
                     Upper('Order_Details'), 1,
                     Upper('Orders') , 2,
                     Upper('Products'), 3, 
                     Upper('Categories'), 4,
                     Upper('Customers'), 5,
                     Upper('Shippers'), 6,
                     Upper('Suppliers'), 7,
                     Upper('Employees'), 8,
                     Upper('CustomerCustomerDemo'),20,
                     Upper('CustomerDemographics'),21,
                     Upper('Region'),22,
                     Upper('Territories'),23,
                     Upper('EmployeeTerritories'),24,
                     0) as DropOrder
            from sys.user_tables
            )
    select table_name from dropTable
    where DropOrder > 0
    Order by DropOrder ;    
    
    cursor c3 is
    	Select sequence_name from sys.user_sequences;
Begin

    Open c1;
    Loop
        Fetch c1 into viewName;
        Exit When C1%NOTFOUND;
        sqlcmd := 'DROP VIEW ' || viewName || '';
       Execute Immediate sqlcmd;
    End Loop;
    Close c1;
   
       
    Open c2;
    Loop
        Fetch c2 into tableName;
        Exit When C2%NOTFOUND;
        sqlcmd := 'DROP TABLE ' || tableName || ' CASCADE CONSTRAINT';
        Execute Immediate sqlcmd;
    End Loop;
    Close c2; 
    
    Open c3;
    Loop
        Fetch c3 into tableName;
        Exit When C3%NOTFOUND;
        sqlcmd := 'DROP SEQUENCE ' || tableName || '';
        Execute Immediate sqlcmd;
    End Loop;
    Close c3; 

End;
/