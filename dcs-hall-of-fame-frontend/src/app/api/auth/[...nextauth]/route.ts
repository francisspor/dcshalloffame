import NextAuth from 'next-auth'
import GoogleProvider from 'next-auth/providers/google'

const handler = NextAuth({
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
    async signIn({ user, account, profile }) {
      // Check if user is in the duanesburgboe group
      if (account?.access_token) {
        try {
          const response = await fetch(
            `https://admin.googleapis.com/admin/directory/v1/groups/duanesburgboe@duanesburg.org/members/${user.email}`,
            {
              headers: {
                'Authorization': `Bearer ${account.access_token}`,
                'Content-Type': 'application/json'
              }
            }
          )

          if (response.ok) {
            return true // User is in the group, allow sign in
          }
        } catch (error) {
          console.error('Error checking group membership:', error)
        }
      }

      return false // User is not in the group, deny sign in
    },
    async jwt({ token, account, profile }) {
      if (account) {
        token.accessToken = account.access_token
      }
      return token
    },
    async session({ session, token }) {
      session.accessToken = token.accessToken
      return session
    }
  },
  pages: {
    signIn: '/admin/login',
    error: '/admin/login'
  },
  session: {
    strategy: 'jwt'
  }
})

export { handler as GET, handler as POST }