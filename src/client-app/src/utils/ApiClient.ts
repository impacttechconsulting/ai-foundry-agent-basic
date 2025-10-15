// ApiClient.ts - Utility for making authenticated API requests
const ApiClient = {
  get: async (url: string) => {
    const token = localStorage.getItem('authToken');
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const response = await fetch(url, {
      method: 'GET',
      headers,
    });

    return response;
  },
  
  post: async (url: string, data?: any) => {
    const token = localStorage.getItem('authToken');
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const response = await fetch(url, {
      method: 'POST',
      headers,
      body: data ? JSON.stringify(data) : undefined,
    });

    return response;
  },
  
  put: async (url: string, data: any) => {
    const token = localStorage.getItem('authToken');
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const response = await fetch(url, {
      method: 'PUT',
      headers,
      body: JSON.stringify(data),
    });

    return response;
  },
  
  delete: async (url: string) => {
    const token = localStorage.getItem('authToken');
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const response = await fetch(url, {
      method: 'DELETE',
      headers,
    });

    return response;
  }
};

export default ApiClient;