import { HallOfFameMember, MemberCategory, ApiResponse } from '@/types/member'
import { findMemberBySlug } from '@/utils/slug'

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5232/api/v1'

class ApiService {
  private async request<T>(endpoint: string, options?: RequestInit): Promise<ApiResponse<T>> {
    try {
      const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        headers: {
          'Content-Type': 'application/json',
          ...options?.headers,
        },
        ...options,
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      const data = await response.json()
      return { data }
    } catch (error) {
      console.error('API request failed:', error)
      return { error: error instanceof Error ? error.message : 'An unknown error occurred' }
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

    return { data: member }
  }

  // Create new member
  async createMember(member: Omit<HallOfFameMember, 'id' | 'createdAt' | 'updatedAt'>): Promise<ApiResponse<string>> {
    return this.request<string>('/halloffame', {
      method: 'POST',
      body: JSON.stringify(member),
    })
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