from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import psycopg2
import psycopg2.extras
from datetime import datetime
import os
import logging
import random
from dotenv import load_dotenv
from typing import Optional, List, Dict, Any

# Load environment variables
load_dotenv()

# Setup logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI(
    title="AI Support Analysis Service",
    version="2.0.0",
    description="AI-powered customer support ticket analysis with sentiment, categorization, and analytics"
)

# CORS middleware - Allow all origins for development
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Database configuration
DB_CONFIG = {
    "host": os.getenv("DB_HOST", "localhost"),
    "port": os.getenv("DB_PORT", "5432"),
    "database": os.getenv("DB_NAME", "customersupportdb"),
    "user": os.getenv("DB_USER", "support_admin"),
    "password": os.getenv("DB_PASSWORD", "Ubo@1234")
}

def get_db_connection():
    """Get a database connection"""
    try:
        conn = psycopg2.connect(**DB_CONFIG)
        return conn
    except Exception as e:
        logger.error(f"Database connection error: {e}")
        raise

def test_db_connection():
    """Test database connection"""
    try:
        conn = get_db_connection()
        conn.close()
        return True
    except Exception as e:
        logger.error(f"Database test failed: {e}")
        return False

# Pydantic Models
class TicketAnalysisRequest(BaseModel):
    ticket_id: int
    subject: str
    body: str
    customer_id: Optional[int] = None

class TicketAnalysisResponse(BaseModel):
    ticket_id: int
    sentiment_score: float
    sentiment_label: str
    category: str
    keywords: Optional[str] = None
    confidence: Optional[float] = None
    processed_at: datetime

class BulkAnalysisRequest(BaseModel):
    tickets: List[TicketAnalysisRequest]

class BulkAnalysisResponse(BaseModel):
    results: List[TicketAnalysisResponse]
    total_processed: int
    processing_time_seconds: float

class AnalyticsSummaryResponse(BaseModel):
    total_tickets: int
    sentiment_distribution: List[Dict[str, Any]]
    category_distribution: List[Dict[str, Any]]
    average_sentiment: float
    top_category: Optional[str]
    timestamp: datetime

@app.on_event("startup")
async def startup():
    """Test database connection on startup"""
    try:
        conn = get_db_connection()
        conn.close()
        logger.info("✅ Database connection successful")
    except Exception as e:
        logger.error(f"❌ Database connection failed: {e}")

@app.get("/")
async def root():
    return {
        "message": "AI Support Analysis Service is running",
        "version": "2.0.0",
        "timestamp": datetime.now().isoformat(),
        "database": "connected" if test_db_connection() else "disconnected",
        "endpoints": {
            "health": "/health",
            "analyze": "/analyze (POST)",
            "bulk_analyze": "/analyze/bulk (POST)",
            "ticket_analysis": "/tickets/{ticket_id}/analysis",
            "analytics_summary": "/analytics/summary",
            "sentiment_trend": "/analytics/sentiment-trend",
            "category_distribution": "/analytics/category-distribution",
            "tickets": "/tickets"
        }
    }

@app.get("/health")
async def health_check():
    """Health check endpoint"""
    try:
        # Test database connection
        db_status = "connected" if test_db_connection() else "disconnected"
        
        # Test if we can query
        if db_status == "connected":
            conn = get_db_connection()
            cursor = conn.cursor()
            cursor.execute("SELECT 1")
            cursor.close()
            conn.close()
        
        return {
            "status": "healthy",
            "timestamp": datetime.now().isoformat(),
            "database": db_status,
            "service": "running"
        }
    except Exception as e:
        logger.error(f"Health check failed: {e}")
        return {
            "status": "unhealthy",
            "timestamp": datetime.now().isoformat(),
            "database": "disconnected",
            "service": "running",
            "error": str(e)
        }

def perform_analysis(text: str) -> Dict[str, Any]:
    """
    Perform AI analysis on ticket text.
    This is a mock implementation - replace with actual AI/ML logic.
    """
    text_lower = text.lower()
    
    # Sentiment analysis
    positive_words = ["love", "great", "awesome", "perfect", "excellent", "happy", "satisfied", "good"]
    negative_words = ["bad", "terrible", "issue", "problem", "error", "fail", "broken", "wrong", "complaint"]
    
    positive_score = sum(1 for word in positive_words if word in text_lower)
    negative_score = sum(1 for word in negative_words if word in text_lower)
    
    if positive_score > negative_score:
        sentiment_label = "Positive"
        sentiment_score = 0.5 + (positive_score / (positive_score + negative_score + 1)) * 0.5
    elif negative_score > positive_score:
        sentiment_label = "Negative"
        sentiment_score = -0.5 - (negative_score / (positive_score + negative_score + 1)) * 0.5
    else:
        sentiment_label = "Neutral"
        sentiment_score = random.uniform(-0.2, 0.2)
    
    # Ensure score is within bounds
    sentiment_score = max(-1.0, min(1.0, sentiment_score))
    confidence = abs(sentiment_score) * 0.8 + 0.2
    
    # Categorization
    categories = {
        "Billing": ["bill", "payment", "charge", "subscription", "invoice", "refund", "price", "cost"],
        "Technical Support": ["error", "bug", "crash", "not working", "issue", "problem", "failed", "exception"],
        "Account Recovery": ["password", "login", "access", "recovery", "reset", "forgot", "locked"],
        "Product Feedback": ["feature", "suggestion", "improve", "enhancement", "request", "like", "useful"],
        "Sales": ["purchase", "buy", "demo", "trial", "upgrade", "plan"],
        "Security": ["security", "breach", "vulnerability", "hack", "compliance"],
        "Performance": ["slow", "fast", "speed", "latency", "response time"]
    }
    
    category_scores = {}
    for category, keywords in categories.items():
        score = sum(1 for keyword in keywords if keyword in text_lower)
        category_scores[category] = score
    
    if category_scores:
        primary_category = max(category_scores, key=category_scores.get)
        category_confidence = min(1.0, category_scores[primary_category] / 3)
    else:
        primary_category = "General"
        category_confidence = 0.3
    
    # Extract keywords
    words = text_lower.split()
    keywords = [word for word in words if len(word) > 4 and word.isalpha()][:5]
    
    return {
        "sentiment_score": round(sentiment_score, 4),
        "sentiment_label": sentiment_label,
        "confidence": round(confidence, 4),
        "category": primary_category,
        "category_confidence": round(category_confidence, 4),
        "keywords": ", ".join(keywords) if keywords else None
    }

@app.post("/analyze", response_model=TicketAnalysisResponse)
async def analyze_ticket(request: TicketAnalysisRequest):
    """
    Analyze a customer support ticket with AI.
    """
    try:
        logger.info(f"Analyzing ticket {request.ticket_id}")
        
        # Combine subject and body for analysis
        full_text = f"{request.subject} {request.body}"
        
        # Perform analysis
        analysis_result = perform_analysis(full_text)
        
        # Save analysis to database
        try:
            conn = get_db_connection()
            cursor = conn.cursor()
            
            cursor.execute("""
                INSERT INTO support.ai_analysis 
                (ticket_id, sentiment_score, sentiment_label, category, keywords, processed_at)
                VALUES (%s, %s, %s, %s, %s, %s)
                ON CONFLICT (ticket_id) 
                DO UPDATE SET 
                    sentiment_score = EXCLUDED.sentiment_score,
                    sentiment_label = EXCLUDED.sentiment_label,
                    category = EXCLUDED.category,
                    keywords = EXCLUDED.keywords,
                    processed_at = EXCLUDED.processed_at
            """, (
                request.ticket_id,
                analysis_result["sentiment_score"],
                analysis_result["sentiment_label"],
                analysis_result["category"],
                analysis_result["keywords"],
                datetime.now()
            ))
            
            conn.commit()
            cursor.close()
            conn.close()
            
            logger.info(f"✅ Analysis saved for ticket {request.ticket_id}")
            
        except Exception as e:
            logger.error(f"Failed to save analysis: {e}")
            # Continue even if save fails
        
        return TicketAnalysisResponse(
            ticket_id=request.ticket_id,
            sentiment_score=analysis_result["sentiment_score"],
            sentiment_label=analysis_result["sentiment_label"],
            category=analysis_result["category"],
            keywords=analysis_result["keywords"],
            confidence=analysis_result["confidence"],
            processed_at=datetime.now()
        )
        
    except Exception as e:
        logger.error(f"Analysis failed: {e}")
        raise HTTPException(status_code=500, detail=f"Analysis failed: {str(e)}")

@app.post("/analyze/bulk", response_model=BulkAnalysisResponse)
async def analyze_bulk_tickets(request: BulkAnalysisRequest):
    """
    Analyze multiple tickets in bulk.
    """
    start_time = datetime.now()
    results = []
    
    for ticket in request.tickets:
        try:
            result = await analyze_ticket(ticket)
            results.append(result)
        except Exception as e:
            logger.error(f"Error analyzing ticket {ticket.ticket_id}: {e}")
            # Add error result
            results.append(TicketAnalysisResponse(
                ticket_id=ticket.ticket_id,
                sentiment_score=0.0,
                sentiment_label="Error",
                category="Error",
                keywords=None,
                confidence=0.0,
                processed_at=datetime.now()
            ))
    
    processing_time = (datetime.now() - start_time).total_seconds()
    
    return BulkAnalysisResponse(
        results=results,
        total_processed=len(results),
        processing_time_seconds=round(processing_time, 2)
    )

@app.get("/tickets/{ticket_id}/analysis")
async def get_ticket_analysis(ticket_id: int):
    """Get stored analysis for a ticket"""
    try:
        conn = get_db_connection()
        cursor = conn.cursor(cursor_factory=psycopg2.extras.RealDictCursor)
        
        cursor.execute("""
            SELECT * FROM support.ai_analysis 
            WHERE ticket_id = %s
        """, (ticket_id,))
        
        result = cursor.fetchone()
        cursor.close()
        conn.close()
        
        if result:
            return dict(result)
        else:
            return {"error": "No analysis found for this ticket", "ticket_id": ticket_id}
            
    except Exception as e:
        logger.error(f"Error fetching analysis: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/analytics/summary", response_model=AnalyticsSummaryResponse)
async def get_analytics_summary():
    """Get analytics summary from database"""
    try:
        conn = get_db_connection()
        cursor = conn.cursor(cursor_factory=psycopg2.extras.RealDictCursor)
        
        # Total tickets
        cursor.execute("SELECT COUNT(*) as total FROM support.tickets")
        total_tickets = cursor.fetchone()["total"]
        
        # Sentiment distribution
        cursor.execute("""
            SELECT sentiment_label, COUNT(*) as count 
            FROM support.ai_analysis 
            GROUP BY sentiment_label
            ORDER BY count DESC
        """)
        sentiment_distribution = cursor.fetchall()
        
        # Category distribution
        cursor.execute("""
            SELECT category, COUNT(*) as count 
            FROM support.ai_analysis 
            GROUP BY category
            ORDER BY count DESC
        """)
        category_distribution = cursor.fetchall()
        
        # Average sentiment
        cursor.execute("SELECT AVG(sentiment_score) as avg_sentiment FROM support.ai_analysis")
        avg_sentiment = cursor.fetchone()["avg_sentiment"]
        
        # Top category
        cursor.execute("""
            SELECT category, COUNT(*) as count 
            FROM support.ai_analysis 
            GROUP BY category 
            ORDER BY count DESC 
            LIMIT 1
        """)
        top_category = cursor.fetchone()
        
        cursor.close()
        conn.close()
        
        return AnalyticsSummaryResponse(
            total_tickets=total_tickets or 0,
            sentiment_distribution=sentiment_distribution or [],
            category_distribution=category_distribution or [],
            average_sentiment=round(avg_sentiment or 0, 4),
            top_category=top_category["category"] if top_category else None,
            timestamp=datetime.now()
        )
        
    except Exception as e:
        logger.error(f"Error fetching analytics: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/analytics/sentiment-trend")
async def get_sentiment_trend(days: int = 30):
    """Get sentiment trend over time"""
    try:
        conn = get_db_connection()
        cursor = conn.cursor(cursor_factory=psycopg2.extras.RealDictCursor)
        
        cursor.execute("""
            SELECT 
                DATE(processed_at) as date,
                AVG(sentiment_score) as avg_sentiment,
                COUNT(*) as ticket_count
            FROM support.ai_analysis
            WHERE processed_at >= CURRENT_DATE - INTERVAL '%s days'
            GROUP BY DATE(processed_at)
            ORDER BY date ASC
        """, (days,))
        
        results = cursor.fetchall()
        cursor.close()
        conn.close()
        
        return {
            "trend": results,
            "days": days,
            "timestamp": datetime.now().isoformat()
        }
        
    except Exception as e:
        logger.error(f"Error fetching sentiment trend: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/analytics/category-distribution")
async def get_category_distribution():
    """Get category distribution with percentages"""
    try:
        conn = get_db_connection()
        cursor = conn.cursor(cursor_factory=psycopg2.extras.RealDictCursor)
        
        cursor.execute("""
            SELECT 
                category,
                COUNT(*) as count,
                ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER(), 2) as percentage
            FROM support.ai_analysis
            GROUP BY category
            ORDER BY count DESC
        """)
        
        results = cursor.fetchall()
        cursor.close()
        conn.close()
        
        return {
            "distribution": results,
            "timestamp": datetime.now().isoformat()
        }
        
    except Exception as e:
        logger.error(f"Error fetching category distribution: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/tickets")
async def get_all_tickets(limit: int = 50, offset: int = 0):
    """Get all tickets with their analysis"""
    try:
        conn = get_db_connection()
        cursor = conn.cursor(cursor_factory=psycopg2.extras.RealDictCursor)
        
        cursor.execute("""
            SELECT 
                t.ticket_id,
                t.subject,
                t.status,
                t.priority,
                t.created_at,
                t.updated_at,
                c.name as customer_name,
                c.email as customer_email,
                a.sentiment_score,
                a.sentiment_label,
                a.category,
                a.keywords,
                a.processed_at as analyzed_at
            FROM support.tickets t
            LEFT JOIN support.customers c ON t.customer_id = c.customer_id
            LEFT JOIN support.ai_analysis a ON t.ticket_id = a.ticket_id
            ORDER BY t.created_at DESC
            LIMIT %s OFFSET %s
        """, (limit, offset))
        
        results = cursor.fetchall()
        cursor.close()
        conn.close()
        
        return {
            "tickets": results,
            "total": len(results),
            "limit": limit,
            "offset": offset,
            "timestamp": datetime.now().isoformat()
        }
        
    except Exception as e:
        logger.error(f"Error fetching tickets: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/tickets/unanalyzed")
async def get_unanalyzed_tickets():
    """Get tickets that haven't been analyzed yet"""
    try:
        conn = get_db_connection()
        cursor = conn.cursor(cursor_factory=psycopg2.extras.RealDictCursor)
        
        cursor.execute("""
            SELECT 
                t.ticket_id,
                t.subject,
                t.body,
                t.status,
                t.priority,
                t.created_at,
                c.name as customer_name
            FROM support.tickets t
            LEFT JOIN support.customers c ON t.customer_id = c.customer_id
            LEFT JOIN support.ai_analysis a ON t.ticket_id = a.ticket_id
            WHERE a.analysis_id IS NULL
            ORDER BY t.created_at ASC
            LIMIT 100
        """)
        
        results = cursor.fetchall()
        cursor.close()
        conn.close()
        
        return {
            "unanalyzed_tickets": results,
            "count": len(results),
            "timestamp": datetime.now().isoformat()
        }
        
    except Exception as e:
        logger.error(f"Error fetching unanalyzed tickets: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/db-info")
async def get_database_info():
    """Get database information"""
    try:
        conn = get_db_connection()
        cursor = conn.cursor(cursor_factory=psycopg2.extras.RealDictCursor)
        
        # Database size
        cursor.execute("SELECT pg_database_size('customersupportdb')/1024/1024 AS size_mb")
        size = cursor.fetchone()["size_mb"]
        
        # Table counts
        cursor.execute("""
            SELECT 
                'customers' as table_name, COUNT(*) as count FROM support.customers
            UNION ALL
            SELECT 'tickets', COUNT(*) FROM support.tickets
            UNION ALL
            SELECT 'ai_analysis', COUNT(*) FROM support.ai_analysis
            UNION ALL
            SELECT 'agents', COUNT(*) FROM support.agents
        """)
        table_counts = cursor.fetchall()
        
        cursor.close()
        conn.close()
        
        return {
            "database": "customersupportdb",
            "size_mb": size,
            "table_counts": table_counts,
            "timestamp": datetime.now().isoformat()
        }
        
    except Exception as e:
        logger.error(f"Error fetching database info: {e}")
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(
        "app:app",
        host="0.0.0.0",
        port=8000,
        reload=True,
        log_level="info"
    )