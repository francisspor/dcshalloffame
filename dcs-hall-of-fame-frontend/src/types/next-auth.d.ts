// Remove unused NextAuth if not used

declare module 'next-auth' {
  interface Session {
    accessToken?: string
    user: {
      id: string
      name: string
      email: string
      image?: string
    }
  }
}

declare module 'next-auth/jwt' {
  interface JWT {
    accessToken?: string
  }
}