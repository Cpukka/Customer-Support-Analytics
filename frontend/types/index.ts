export interface Customer {
  customerID: number;
  name: string;
  email: string;
  company?: string;
  createdAt: string;
  updatedAt: string;
}

export interface AIAnalysis {
  analysisID: number;
  ticketID: number;
  sentimentScore: number;
  sentimentLabel: 'Positive' | 'Neutral' | 'Negative';
  category: string;
  keywords?: string;
  processedAt: string;
}

export interface Ticket {
  ticketID: number;
  customerID: number;
  subject: string;
  body: string;
  status: 'Open' | 'In Progress' | 'Closed';
  priority: 'Low' | 'Medium' | 'High';
  createdAt: string;
  updatedAt: string;
  customer?: Customer;
  aiAnalysis?: AIAnalysis;
}

export interface AnalyticsSummary {
  totalTickets: number;
  negativeSentimentRate: number;
  averageResolutionTimeHours: number;
  topIssueCategory: string;
  sentimentDistribution: Record<string, number>;
}

export interface CategoryDistribution {
  category: string;
  count: number;
}

export interface UploadTicketRequest {
  customerId: number;
  subject: string;
  body: string;
  priority?: string;
}