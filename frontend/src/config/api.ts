export const API_CONFIG = {
  BASE_URL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000',
  TIMEOUT: 30000
};

export const ENDPOINTS = {
  AUTH: {
    LOGIN: '/api/auth/login',
    REGISTER: '/api/auth/register',
    REFRESH: '/api/auth/refresh',
    PROFILE: '/api/auth/profile'
  },
  SUBSCRIPTION: {
    CURRENT: '/api/subscription/current',
    PLANS: '/api/subscription/plans',
    USAGE: '/api/subscription/usage',
    REMAINING: '/api/subscription/remaining-credits',
    CONSUME: '/api/subscription/consume',
    UPGRADE: '/api/subscription/upgrade',
    ASSIGN_TRIAL: '/api/subscription/assign-trial'
  },
  AI: {
    ANALYZE_CASE: '/api/ai/analyze-case',
    EXTRACT_KEYWORDS: '/api/ai/extract-keywords',
    SCORE_DECISIONS: '/api/ai/score-decisions'
  },
  SEARCH: {
    SEARCH: '/api/search/search',
    HISTORY: '/api/search/history',
    SAVE_DECISION: '/api/search/save-decision'
  },
  PETITION: {
    GENERATE: '/api/document/generate-petition',
    HISTORY: '/api/document/petition-history'
  }
};
