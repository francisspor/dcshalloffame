import { NextRequest, NextResponse } from 'next/server'

export async function GET(request: NextRequest) {
  const { searchParams } = new URL(request.url)
  const email = searchParams.get('email')
  const accessToken = searchParams.get('token')

  if (!email || !accessToken) {
    return NextResponse.json({
      error: 'Missing email or access token',
      usage: 'Use ?email=user@duanesburg.org&token=your_access_token'
    }, { status: 400 })
  }

  try {
    console.log('Testing Admin API access for:', email)

    const response = await fetch(
      `https://admin.googleapis.com/admin/directory/v1/groups/duanesburgboe@duanesburg.org/members/${email}`,
      {
        headers: {
          'Authorization': `Bearer ${accessToken}`,
          'Content-Type': 'application/json'
        }
      }
    )

    const responseText = await response.text()
    let responseData
    try {
      responseData = JSON.parse(responseText)
    } catch {
      responseData = { raw: responseText }
    }

    return NextResponse.json({
      status: response.status,
      statusText: response.statusText,
      headers: Object.fromEntries(response.headers.entries()),
      data: responseData,
      success: response.ok
    })

  } catch (error) {
    console.error('Error testing Admin API:', error)
    return NextResponse.json({
      error: 'Failed to test Admin API',
      details: error instanceof Error ? error.message : String(error)
    }, { status: 500 })
  }
}