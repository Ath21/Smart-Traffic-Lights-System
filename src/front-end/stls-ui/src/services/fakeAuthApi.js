// src/services/fakeAuthApi.js
export async function loginApi({ email, password }) {
  const role = email.includes('admin') ? 'admin'
    : email.includes('operator') ? 'operator'
    : email.includes('user') ? 'user'
    : 'viewer'

  const payload = { sub: '123', email, role }
  const token = btoa(JSON.stringify(payload))
  return { token }
}

export async function registerApi({ email, username, password }) {
  // Demo: create as 'user'
  const payload = { sub: 'u-' + Date.now(), email, username, role: 'user' }
  const token = btoa(JSON.stringify(payload))
  return { token }
}
