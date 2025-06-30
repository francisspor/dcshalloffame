import type { NextApiRequest, NextApiResponse } from 'next'
import { getServerSession } from 'next-auth/next'
import { authOptions } from '@/lib/auth'

// Import your NextAuth config if needed
// import { authOptions } from '../auth/[...nextauth]'

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5232/api/v1'
const JWT_SECRET = process.env.NEXT_PUBLIC_JWT_SECRET || 'your-super-secret-jwt-key-here-make-it-long-and-random-in-production'

console.log('🔧 Environment check:')
console.log('🔧 NEXT_PUBLIC_JWT_SECRET length:', process.env.NEXT_PUBLIC_JWT_SECRET?.length || 0)
console.log('🔧 NEXT_PUBLIC_JWT_SECRET starts with:', process.env.NEXT_PUBLIC_JWT_SECRET?.substring(0, 10) + '...')
console.log('🔧 JWT_SECRET length:', JWT_SECRET?.length || 0)
console.log('🔧 JWT_SECRET starts with:', JWT_SECRET?.substring(0, 10) + '...')

export default async function handler(req: NextApiRequest, res: NextApiResponse) {
  try {
    // Get session from NextAuth
    const session = await getServerSession(req, res, authOptions)

    if (!session || !session.user?.email) {
      return res.status(401).json({ error: 'Unauthorized: No session' })
    }

    // Validate user has admin role
    if (session.user.role !== 'admin') {
      return res.status(403).json({ error: 'Forbidden: Admin access required' })
    }

    // Prepare the proxied request
    const { method, body } = req
    let endpoint = req.query.endpoint
    if (!endpoint || typeof endpoint !== 'string') {
      return res.status(400).json({ error: 'Missing endpoint query param' })
    }
    // Remove leading slash if present
    if (endpoint.startsWith('/')) endpoint = endpoint.slice(1)

    const url = `${API_BASE_URL}/${endpoint}`

    // Forward the request with user info in headers
    const apiRes = await fetch(url, {
      method,
      headers: {
        'Content-Type': 'application/json',
        'X-User-Email': session.user.email,
        'X-User-Role': session.user.role || 'admin',
      },
      body: method !== 'GET' && method !== 'HEAD' ? JSON.stringify(body) : undefined,
    })

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