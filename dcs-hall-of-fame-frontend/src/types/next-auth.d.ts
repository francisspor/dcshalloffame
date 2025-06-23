// Remove unused NextAuth if not used

import 'next-auth'

declare module 'next-auth' {
  interface User {
    role?: string
  }

  interface Session {
    accessToken?: string
    user: {
      id: string
      name: string
      email: string
      image?: string
      role?: string
    }
  }
}

declare module 'next-auth/jwt' {
  interface JWT {
    accessToken?: string
    role?: string
  }
}