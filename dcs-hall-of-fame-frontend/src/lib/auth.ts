import type { NextAuthOptions } from 'next-auth'
import GoogleProvider from 'next-auth/providers/google'

export const authOptions: NextAuthOptions = {
  providers: [
    GoogleProvider({
      clientId: process.env.GOOGLE_CLIENT_ID!,
      clientSecret: process.env.GOOGLE_CLIENT_SECRET!,
      authorization: {
        params: {
          scope: 'openid email profile https://www.googleapis.com/auth/admin.directory.group.member.readonly'
        }
      }
    })
  ],
  callbacks: {
    async signIn({ user, account }) {
      console.log('SignIn callback triggered for:', user.email)
      console.log('Account access token exists:', !!account?.access_token)

      // Check if user is in the duanesburgboe group
      if (account?.access_token) {
        try {
          console.log('Checking group membership for:', user.email)

          const response = await fetch(
            `https://admin.googleapis.com/admin/directory/v1/groups/duanesburgboe@duanesburg.org/members/${user.email}`,
            {
              headers: {
                'Authorization': `Bearer ${account.access_token}`,
                'Content-Type': 'application/json'
              }
            }
          )

          console.log('Group membership check response status:', response.status)

          if (response.ok) {
            console.log('User is in the group, allowing sign in')
            return true // User is in the group, allow sign in
          } else {
            const errorText = await response.text()
            console.log('Group membership check failed:', response.status, errorText)

            // If it's a 404, the user is not in the group
            if (response.status === 404) {
              console.log('User is not in the group, denying sign in')
              return false
            }

            // For other errors (like API not enabled), we might want to allow access
            // or implement a fallback mechanism
            console.log('API error, checking if user has duanesburg.org domain')

            // Fallback: Check if user has duanesburg.org email domain
            if (user.email && user.email.endsWith('@duanesburg.org')) {
              console.log('User has duanesburg.org email, allowing access as fallback')
              return true
            }
          }
        } catch (error) {
          console.error('Error checking group membership:', error)

          // Fallback: Check if user has duanesburg.org email domain
          if (user.email && user.email.endsWith('@duanesburg.org')) {
            console.log('Error occurred, but user has duanesburg.org email, allowing access as fallback')
            return true
          }
        }
      } else {
        console.log('No access token available')
      }

      console.log('Access denied for user:', user.email)
      return false // User is not in the group, deny sign in
    },
    jwt: async ({ token, user }) => {
      if (user) {
        token.role = user.role;
      }
      return token;
    },
    session: async ({ session, token }) => {
      if (token && session.user) {
        session.user.role = token.role;
      }
      return session;
    }
  },
  pages: {
    signIn: '/admin/login',
    error: '/admin/login'
  },
  session: {
    strategy: 'jwt' as const
  },
  debug: process.env.NODE_ENV === 'development'
}