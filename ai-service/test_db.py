import psycopg2
import sys

def test_connection():
    try:
        # Connect to PostgreSQL
        conn = psycopg2.connect(
            host="localhost",
            port="5432",
            database="customersupportdb",
            user="support_admin",
            password="Ubo@1234"
        )
        
        print("✅ Database connection successful!")
        print(f"📊 Connected to: customersupportdb")
        
        cursor = conn.cursor()
        
        # Check tables
        print("\n📋 Tables in support schema:")
        cursor.execute("""
            SELECT table_name 
            FROM information_schema.tables 
            WHERE table_schema = 'support' 
            ORDER BY table_name;
        """)
        tables = cursor.fetchall()
        for table in tables:
            print(f"  - {table[0]}")
        
        # Check sample data
        print("\n👤 Sample Customers:")
        cursor.execute("SELECT customer_id, name, email FROM support.customers LIMIT 3;")
        customers = cursor.fetchall()
        for customer in customers:
            print(f"  ID: {customer[0]}, Name: {customer[1]}, Email: {customer[2]}")
        
        # Check ticket count
        cursor.execute("SELECT COUNT(*) FROM support.tickets;")
        ticket_count = cursor.fetchone()[0]
        print(f"\n🎫 Total Tickets: {ticket_count}")
        
        # Check database size
        cursor.execute("SELECT pg_database_size('customersupportdb')/1024/1024 AS size_mb;")
        size = cursor.fetchone()[0]
        print(f"💾 Database Size: {size} MB")
        
        cursor.close()
        conn.close()
        
        print("\n✅ All tests passed!")
        
    except Exception as e:
        print(f"❌ Error: {e}")
        print("\n💡 Troubleshooting Tips:")
        print("1. Make sure PostgreSQL is running")
        print("2. Verify the database exists")
        print("3. Check username and password")
        print("4. Ensure port 5432 is not blocked")
        sys.exit(1)

if __name__ == "__main__":
    test_connection()