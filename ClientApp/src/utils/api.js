// src/utils/api.js

/**
 * 發送帶有 JWT Token 的 API 請求
 * @param {string} url - API 端點
 * @param {object} options - fetch 選項
 * @returns {Promise<Response>}
 */
export async function apiRequest(url, options = {}) {
    const token = localStorage.getItem('authToken')

    const defaultHeaders = {
        'Content-Type': 'application/json',
    }

    // 如果有 token，自動加入 Authorization header
    if (token) {
        defaultHeaders['Authorization'] = `Bearer ${token}`
    }

    const config = {
        ...options,
        headers: {
            ...defaultHeaders,
            ...options.headers,
        },
    }

    try {
        const response = await fetch(url, config)

        // 如果是 401 未授權，可能是 token 過期，清除本地資料並導向登入頁
        if (response.status === 401) {
            console.warn('Token expired or invalid, redirecting to login')
            localStorage.removeItem('authToken')
            localStorage.removeItem('adminUser')

            // 如果當前不在登入頁面，則導向登入頁面
            if (window.location.pathname !== '/login') {
                window.location.href = '/login'
            }
        }

        return response
    } catch (error) {
        console.error('API request failed:', error)
        throw error
    }
}

/**
 * 發送 GET 請求
 * @param {string} url - API 端點
 * @returns {Promise<any>}
 */
export async function apiGet(url) {
    const response = await apiRequest(url, { method: 'GET' })
    if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
    }
    return response.json()
}

/**
 * 發送 POST 請求
 * @param {string} url - API 端點
 * @param {any} data - 請求資料
 * @returns {Promise<any>}
 */
export async function apiPost(url, data) {
    const response = await apiRequest(url, {
        method: 'POST',
        body: JSON.stringify(data),
    })
    if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
    }
    return response.json()
}

/**
 * 發送 DELETE 請求
 * @param {string} url - API 端點
 * @returns {Promise<any>}
 */
export async function apiDelete(url) {
    const response = await apiRequest(url, { method: 'DELETE' })
    if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
    }
    return response.json()
}

/**
 * 驗證當前 Token 是否有效
 * @returns {Promise<boolean>}
 */
export async function verifyToken() {
    try {
        const response = await apiRequest('/api/Auth/verify', { method: 'GET' })
        return response.ok
    } catch (error) {
        console.error('Token verification failed:', error)
        return false
    }
}

/**
 * 檢查使用者是否已登入
 * @returns {boolean}
 */
export function isAuthenticated() {
    const token = localStorage.getItem('authToken')
    const user = localStorage.getItem('adminUser')
    return !!(token && user)
}