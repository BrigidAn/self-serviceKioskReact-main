const API_BASE = "https://localhost:5016/api";

const api = {
  // Helper: get token
  getToken() {
    return localStorage.getItem("token");
  },

  // Helper: create headers
  getHeaders() {
    const token = this.getToken();

    return {
      "Content-Type": "application/json",
      ...(token && { Authorization: `Bearer ${token}` })
    };
  },

  // GET request
  async get(endpoint) {
    const res = await fetch(`${API_BASE}/${endpoint}`, {
      headers: this.getHeaders(),
    });

    const data = await res.json().catch(() => null);

    if (!res.ok) {
      throw new Error(data?.message || "GET request failed");
    }

    return data;
  },


  // POST request
  async post(endpoint, body = {}) {
    const res = await fetch(`${API_BASE}/${endpoint}`, {
      method: "POST",
      headers: this.getHeaders(),
      body: JSON.stringify(body),
    });

    const data = await res.json().catch(() => null);

    if (!res.ok) {
      throw new Error(data?.message || "POST request failed");
    }

    return data;
  },

  // PUT request
  async put(endpoint, body = {}) {
    const res = await fetch(`${API_BASE}/${endpoint}`, {
      method: "PUT",
      headers: this.getHeaders(),
      body: JSON.stringify(body),
    });

    const data = await res.json().catch(() => null);

    if (!res.ok) {
      throw new Error(data?.message || "PUT request failed");
    }

    return data;
  },

//delete
  async delete(endpoint) {
    const res = await fetch(`${API_BASE}/${endpoint}`, {
      method: "DELETE",
      headers: this.getHeaders(),
    });

    const data = await res.json().catch(() => null);

    if (!res.ok) {
      throw new Error(data?.message || "DELETE request failed");
    }

    return data;
  },
};

export default api;
