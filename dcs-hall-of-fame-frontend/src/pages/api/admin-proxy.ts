import type { NextApiRequest, NextApiResponse } from 'next'
import { getServerSession } from 'next-auth/next'
import jwt from 'jsonwebtoken'
import { authOptions } from '@/lib/auth'

// Import your NextAuth config if needed
// import { authOptions } from '../auth/[...nextauth]'

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5232/api/v1'
const JWT_SECRET = process.env.NEXT_PUBLIC_JWT_SECRET || 'your-super-secret-jwt-key-here-make-it-long-and-random-in-production'

export default async function handler(req: NextApiRequest, res: NextApiResponse) {
  console.log('🔍 Admin proxy called:', req.method, req.query.endpoint)
  console.log('🔍 Request headers:', Object.keys(req.headers))
  console.log('🔍 Authorization header:', req.headers.authorization ? 'Present' : 'Missing')

  try {
    // Get session from NextAuth
    console.log('🔍 Attempting to get session...')
    const session = await getServerSession(req, res, authOptions)
    console.log('🔍 Session found:', !!session)
    console.log('🔍 Session user:', session?.user?.email)
    console.log('🔍 Session user role:', session?.user?.role)
    console.log('🔍 Full session object:', JSON.stringify(session, null, 2))

    if (!session || !session.user?.email) {
      console.log('❌ No session found or no user email')
      console.log('❌ Session object:', session)
      return res.status(401).json({ error: 'Unauthorized: No session' })
    }

    console.log('✅ Session validated, generating JWT...')

    // Generate JWT
    const payload = {
      sub: session.user.email,
      email: session.user.email,
      "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": 'admin',
      iss: 'dcs-hall-of-fame',
      aud: 'dcs-hall-of-fame-api',
      iat: Math.floor(Date.now() / 1000),
      exp: Math.floor(Date.now() / 1000) + (60 * 60), // 1 hour
    }

    console.log('🔍 JWT payload:', JSON.stringify(payload, null, 2))
    console.log('🔍 JWT secret length:', JWT_SECRET?.length || 0)
    console.log('🔍 JWT secret starts with:', JWT_SECRET?.substring(0, 10) + '...')

    const token = jwt.sign(payload, JWT_SECRET)
    console.log('✅ JWT generated:', token.substring(0, 50) + '...')

    // Decode the token to verify it was created correctly
    try {
      const decoded = jwt.verify(token, JWT_SECRET)
      console.log('🔍 JWT decoded successfully:', JSON.stringify(decoded, null, 2))
    } catch (decodeError) {
      console.error('❌ JWT decode error:', decodeError)
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
    console.log('🔍 Proxying to:', url)
    console.log('🔍 Request method:', method)
    console.log('🔍 Request body:', body)
    console.log('🔍 Authorization header being sent:', `Bearer ${token.substring(0, 20)}...`)

    // Forward the request
    const apiRes = await fetch(url, {
      method,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
      },
      body: method !== 'GET' && method !== 'HEAD' ? JSON.stringify(body) : undefined,
    })

    console.log('🔍 API response status:', apiRes.status)
    console.log('🔍 API response headers:', Object.fromEntries(apiRes.headers.entries()))

    // Forward status and body
    const contentType = apiRes.headers.get('content-type')
    res.status(apiRes.status)

    if (contentType && contentType.includes('application/json')) {
      const data = await apiRes.json()
      console.log('🔍 API response data:', JSON.stringify(data, null, 2))
      res.json(data)
    } else {
      const text = await apiRes.text()
      console.log('🔍 API response text:', text)
      res.send(text)
    }
  } catch (error) {
    console.error('Proxy error:', error)
    res.status(500).json({ error: 'Proxy error', details: (error as Error).message })
  }
}