import jwt from 'jsonwebtoken'

const JWT_SECRET = process.env.JWT_SECRET || 'your-super-secret-jwt-key-here-make-it-long-and-random-in-production'

export interface JWTPayload {
  sub: string
  email: string
  role: string
  iat?: number
  exp?: number
}

export function generateJWTToken(payload: Omit<JWTPayload, 'iat' | 'exp'>): string {
  return jwt.sign(payload, JWT_SECRET, {
    expiresIn: '1h',
    issuer: 'dcs-hall-of-fame',
    audience: 'dcs-hall-of-fame-api'
  })
}

export function verifyJWTToken(token: string): JWTPayload | null {
  try {
    return jwt.verify(token, JWT_SECRET) as JWTPayload
  } catch (error) {
    console.error('JWT verification failed:', error)
    return null
  }
}