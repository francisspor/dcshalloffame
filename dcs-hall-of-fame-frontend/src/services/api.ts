import { HallOfFameMember, MemberCategory, ApiResponse } from '@/types/member'
import { findMemberBySlug } from '@/utils/slug'
import { getSession } from 'next-auth/react'
import jwt from 'jsonwebtoken'

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'https://dcs-hall-of-fame-api-213912099731.us-central1.run.app/api/v1'

console.log('🔧 API_BASE_URL:', API_BASE_URL);
console.log('🔧 Environment variable NEXT_PUBLIC_API_URL:', process.env.NEXT_PUBLIC_API_URL);

// JWT secret key - should match the one in your API configuration
const JWT_SECRET = process.env.NEXT_PUBLIC_JWT_SECRET || 'your-super-secret-jwt-key-change-this-in-production'

// Helper to determine if a method is an admin action
const isAdminMethod = (method?: string) => {
  return method === 'POST' || method === 'PUT' || method === 'DELETE'
}

class ApiService {
  private async getAuthHeaders(): Promise<HeadersInit> {
    const session = await getSession()
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    }

    if (session?.user?.email) {
      try {
        // Generate JWT token for API authentication
        const payload = {
          sub: session.user.email,
          email: session.user.email,
          "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": 'admin',
          iss: 'dcs-hall-of-fame',
          aud: 'dcs-hall-of-fame-api',
          iat: Math.floor(Date.now() / 1000),
          exp: Math.floor(Date.now() / 1000) + (60 * 60) // 1 hour expiration
        }

        const jwtToken = jwt.sign(payload, JWT_SECRET, {
          algorithm: 'HS256',
          header: {
            typ: 'JWT',
            alg: 'HS256'
          }
        })
        headers['Authorization'] = `Bearer ${jwtToken}`
        console.log('JWT token generated for:', session.user.email)
      } catch (error) {
        console.error('Failed to generate JWT token:', error)
        // Continue without token for read operations
      }
    }

    return headers
  }

  // For GET requests, go directly to the .NET API
  // For admin actions, go through the Next.js proxy
  private async request<T>(endpoint: string, options?: RequestInit): Promise<ApiResponse<T>> {
    try {
      const method = options?.method || 'GET'
      let response: Response
      if (isAdminMethod(method)) {
        // Use Next.js API proxy for admin actions
        const proxyUrl = `/api/admin-proxy?endpoint=${encodeURIComponent(endpoint.replace(/^\//, ''))}`
        response = await fetch(proxyUrl, {
          method,
          headers: {
            'Content-Type': 'application/json',
            ...options?.headers,
          },
          body: isAdminMethod(method) ? options?.body : undefined,
        })
      } else {
        // Public GET requests go directly to the .NET API
        response = await fetch(`${API_BASE_URL}${endpoint}`, {
          method: 'GET',
          mode: 'cors',
          credentials: 'omit',
          headers: {
            'Content-Type': 'application/json',
          },
        })
      }

      if (!response.ok) {
        if (response.status === 401) {
          throw new Error('Unauthorized - Please log in as an admin')
        }
        if (response.status === 403) {
          throw new Error('Forbidden - Admin access required')
        }
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      // Handle empty responses (like HTTP 204 No Content)
      if (response.status === 204 || response.headers.get('content-length') === '0') {
        return { data: undefined as T }
      }

      const data = await response.json()
      return { data }
    } catch (error) {
      console.error('API request failed:', error)
      if (error instanceof Error) {
        return { error: error.message }
      }
      return { error: 'An unknown error occurred' }
    }
  }

  // Get all members
  async getAllMembers(): Promise<ApiResponse<HallOfFameMember[]>> {
    return this.request<HallOfFameMember[]>('/halloffame')
  }

  // Get members by category
  async getMembersByCategory(category: MemberCategory): Promise<ApiResponse<HallOfFameMember[]>> {
    return this.request<HallOfFameMember[]>(`/halloffame/category/${category}`)
  }

  // Get member by ID
  async getMemberById(id: string): Promise<ApiResponse<HallOfFameMember>> {
    return this.request<HallOfFameMember>(`/halloffame/${id}`)
  }

  // Get member by slug
  async getMemberBySlug(slug: string): Promise<ApiResponse<HallOfFameMember>> {
    const allMembers = await this.getAllMembers()

    if (allMembers.error || !allMembers.data) {
      return { error: allMembers.error || 'Failed to fetch members' }
    }

    const member = findMemberBySlug(allMembers.data, slug)

    if (!member) {
      return { error: 'Member not found' }
    }

    return { data: member as HallOfFameMember }
  }

  // Create new member
  async createMember(member: Omit<HallOfFameMember, 'id' | 'createdAt' | 'updatedAt'>): Promise<ApiResponse<string>> {
    const response = await this.request<{ id: string }>('/halloffame', {
      method: 'POST',
      body: JSON.stringify(member),
    })

    if (response.error) {
      return { error: response.error }
    }

    return { data: response.data?.id || '' }
  }

  // Update member
  async updateMember(id: string, member: Partial<HallOfFameMember>): Promise<ApiResponse<void>> {
    return this.request<void>(`/halloffame/${id}`, {
      method: 'PUT',
      body: JSON.stringify(member),
    })
  }

  // Delete member
  async deleteMember(id: string): Promise<ApiResponse<void>> {
    return this.request<void>(`/halloffame/${id}`, {
      method: 'DELETE',
    })
  }

  // Get statistics
  async getStatistics(): Promise<ApiResponse<{
    totalMembers: number;
    staffMembers: number;
    alumniMembers: number;
    categories: { [key: string]: number };
  }>> {
    const allMembers = await this.getAllMembers()

    if (allMembers.error || !allMembers.data) {
      return { error: allMembers.error || 'Failed to fetch statistics' }
    }

    const staffMembers = allMembers.data.filter(m => m.category === MemberCategory.Staff)
    const alumniMembers = allMembers.data.filter(m => m.category === MemberCategory.Alumni)

    // Count by graduation year for alumni
    const categories: { [key: string]: number } = {}
    alumniMembers.forEach(member => {
      if (member.graduationYear) {
        const decade = Math.floor(member.graduationYear / 10) * 10
        const decadeKey = `${decade}s`
        categories[decadeKey] = (categories[decadeKey] || 0) + 1
      }
    })

    return {
      data: {
        totalMembers: allMembers.data.length,
        staffMembers: staffMembers.length,
        alumniMembers: alumniMembers.length,
        categories
      }
    }
  }
}

export const apiService = new ApiService()