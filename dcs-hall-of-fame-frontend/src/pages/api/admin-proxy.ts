import type { NextApiRequest, NextApiResponse } from 'next'
import { getServerSession } from 'next-auth/next'
import jwt from 'jsonwebtoken'

// Import your NextAuth config if needed
// import { authOptions } from '../auth/[...nextauth]'

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5232/api/v1'
const JWT_SECRET = process.env.NEXT_PUBLIC_JWT_SECRET || 'your-super-secret-jwt-key-here-make-it-long-and-random-in-production'

export default async function handler(req: NextApiRequest, res: NextApiResponse) {
  console.log('Admin proxy called:', req.method, req.query.endpoint)

  // Get session from NextAuth
  // If you have a custom authOptions, pass it to getServerSession
  const session = await getServerSession(req, res, {})
  console.log('Session:', session?.user?.email)

  if (!session || !session.user?.email) {
    console.log('No session found')
    return res.status(401).json({ error: 'Unauthorized: No session' })
  }

  // Optionally, check for admin role in your session
  // For now, assume all authenticated users are admins

  // Generate JWT
  const payload = {
    sub: session.user.email,
    email: session.user.email,
    role: 'admin',
    iss: 'dcs-hall-of-fame',
    aud: 'dcs-hall-of-fame-api',
    iat: Math.floor(Date.now() / 1000),
    exp: Math.floor(Date.now() / 1000) + (60 * 60), // 1 hour
  }
  const token = jwt.sign(payload, JWT_SECRET)
  console.log('JWT generated:', token.substring(0, 50) + '...')
  console.log('JWT payload:', payload)

  // Prepare the proxied request
  const { method, body } = req
  let endpoint = req.query.endpoint
  if (!endpoint || typeof endpoint !== 'string') {
    return res.status(400).json({ error: 'Missing endpoint query param' })
  }
  // Remove leading slash if present
  if (endpoint.startsWith('/')) endpoint = endpoint.slice(1)

  const url = `${API_BASE_URL}/${endpoint}`
  console.log('Proxying to:', url)

  // Forward the request
  try {
    const apiRes = await fetch(url, {
      method,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
      },
      body: method !== 'GET' && method !== 'HEAD' ? JSON.stringify(body) : undefined,
    })

    console.log('API response status:', apiRes.status)

    // Forward status and body
    const contentType = apiRes.headers.get('content-type')
    res.status(apiRes.status)
    if (contentType && contentType.includes('application/json')) {
      const data = await apiRes.json()
      res.json(data)
    } else {
      const text = await apiRes.text()
      res.send(text)
    }
  } catch (error) {
    console.error('Proxy error:', error)
    res.status(500).json({ error: 'Proxy error', details: (error as Error).message })
  }
}