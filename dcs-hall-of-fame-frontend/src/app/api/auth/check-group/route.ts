import { NextRequest, NextResponse } from 'next/server'
import { getServerSession } from 'next-auth'

export async function POST(request: NextRequest) {
  try {
    const session = await getServerSession()

    if (!session) {
      return NextResponse.json({ error: 'Unauthorized' }, { status: 401 })
    }

    const { email } = await request.json()

    if (email !== session.user.email) {
      return NextResponse.json({ error: 'Forbidden' }, { status: 403 })
    }

    // For now, we'll use a simple check. In production, you'd want to verify against Google Admin API
    // This is a placeholder - you'll need to implement the actual Google Admin API check
    const isAuthorized = email.endsWith('@duanesburg.org')

    if (isAuthorized) {
      return NextResponse.json({ authorized: true })
    } else {
      return NextResponse.json({ error: 'Not authorized' }, { status: 403 })
    }
  } catch (error) {
    console.error('Error checking group membership:', error)
    return NextResponse.json({ error: 'Internal server error' }, { status: 500 })
  }
}