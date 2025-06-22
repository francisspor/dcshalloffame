'use client'

import { useState, useMemo } from 'react'
import { HallOfFameMember, MemberCategory } from '@/types/member'
import { generateSlug } from '@/utils/slug'
import Link from 'next/link'

interface MemberTableProps {
  members: HallOfFameMember[]
  category: MemberCategory
  loading?: boolean
  error?: string
}

export default function MemberTable({ members, category, loading, error }: MemberTableProps) {
  const [searchTerm, setSearchTerm] = useState('')
  const [sortBy, setSortBy] = useState<'name' | 'inductionYear' | 'graduationYear'>('name')
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc')

  const filteredAndSortedMembers = useMemo(() => {
    let filtered = members.filter(member =>
      member.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      member.biography.toLowerCase().includes(searchTerm.toLowerCase()) ||
      member.achievements.some(achievement =>
        achievement.toLowerCase().includes(searchTerm.toLowerCase())
      )
    )

    filtered.sort((a, b) => {
      let aValue: string | number
      let bValue: string | number

      switch (sortBy) {
        case 'name':
          aValue = a.name
          bValue = b.name
          break
        case 'inductionYear':
          aValue = a.inductionYear
          bValue = b.inductionYear
          break
        case 'graduationYear':
          aValue = a.graduationYear || 0
          bValue = b.graduationYear || 0
          break
        default:
          aValue = a.name
          bValue = b.name
      }

      if (typeof aValue === 'string' && typeof bValue === 'string') {
        return sortOrder === 'asc'
          ? aValue.localeCompare(bValue)
          : bValue.localeCompare(aValue)
      } else {
        return sortOrder === 'asc'
          ? (aValue as number) - (bValue as number)
          : (bValue as number) - (aValue as number)
      }
    })

    return filtered
  }, [members, searchTerm, sortBy, sortOrder])

  const handleSort = (field: 'name' | 'inductionYear' | 'graduationYear') => {
    if (sortBy === field) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc')
    } else {
      setSortBy(field)
      setSortOrder('asc')
    }
  }

  const getSortIcon = (field: 'name' | 'inductionYear' | 'graduationYear') => {
    if (sortBy !== field) return null

    return sortOrder === 'asc' ? '↑' : '↓'
  }

  if (loading) {
    return (
      <div className="text-center py-12">
        <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <p className="mt-2 text-gray-600">Loading members...</p>
      </div>
    )
  }

  if (error) {
    return (
      <div className="bg-red-50 border border-red-200 rounded-lg p-4">
        <p className="text-red-600">Error loading members: {error}</p>
      </div>
    )
  }

  return (
    <div className="space-y-4">
      {/* Search and Sort Controls */}
      <div className="flex flex-col sm:flex-row gap-4 items-start sm:items-center justify-between">
        <div className="flex-1 max-w-md">
          <label htmlFor="search" className="sr-only">Search members</label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <svg className="h-5 w-5 text-gray-400" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z" clipRule="evenodd" />
              </svg>
            </div>
            <input
              type="text"
              id="search"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:placeholder-gray-400 focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Search members..."
            />
          </div>
        </div>

        <div className="flex items-center space-x-2 text-sm text-gray-600">
          <span>Sort by:</span>
          <select
            value={`${sortBy}-${sortOrder}`}
            onChange={(e) => {
              const [field, order] = e.target.value.split('-') as [string, 'asc' | 'desc']
              setSortBy(field as 'name' | 'inductionYear' | 'graduationYear')
              setSortOrder(order)
            }}
            className="border border-gray-300 rounded px-2 py-1 focus:outline-none focus:ring-1 focus:ring-blue-500"
          >
            <option value="name-asc">Name A-Z</option>
            <option value="name-desc">Name Z-A</option>
            <option value="inductionYear-asc">Induction Year (Oldest)</option>
            <option value="inductionYear-desc">Induction Year (Newest)</option>
            {category === MemberCategory.Alumni && (
              <>
                <option value="graduationYear-asc">Graduation Year (Oldest)</option>
                <option value="graduationYear-desc">Graduation Year (Newest)</option>
              </>
            )}
          </select>
        </div>
      </div>

      {/* Results Count */}
      <div className="text-sm text-gray-600">
        Showing {filteredAndSortedMembers.length} of {members.length} members
        {searchTerm && ` matching "${searchTerm}"`}
      </div>

      {/* Table */}
      <div className="bg-white rounded-lg shadow-md overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th
                className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
                onClick={() => handleSort('name')}
              >
                <div className="flex items-center space-x-1">
                  <span>Name</span>
                  <span className="text-gray-400">{getSortIcon('name')}</span>
                </div>
              </th>
              <th
                className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
                onClick={() => handleSort('inductionYear')}
              >
                <div className="flex items-center space-x-1">
                  <span>Induction Year</span>
                  <span className="text-gray-400">{getSortIcon('inductionYear')}</span>
                </div>
              </th>
              {category === MemberCategory.Alumni && (
                <th
                  className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
                  onClick={() => handleSort('graduationYear')}
                >
                  <div className="flex items-center space-x-1">
                    <span>Graduation Year</span>
                    <span className="text-gray-400">{getSortIcon('graduationYear')}</span>
                  </div>
                </th>
              )}
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {filteredAndSortedMembers.length === 0 ? (
              <tr>
                <td colSpan={category === MemberCategory.Alumni ? 4 : 3} className="px-6 py-12 text-center text-gray-500">
                  {searchTerm ? 'No members found matching your search.' : 'No members found.'}
                </td>
              </tr>
            ) : (
              filteredAndSortedMembers.map((member) => (
                <tr key={member.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-medium text-gray-900">{member.name}</div>
                    {member.biography.length > 100 && (
                      <div className="text-sm text-gray-500 truncate max-w-xs">
                        {member.biography.substring(0, 100)}...
                      </div>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {member.inductionYear}
                  </td>
                  {category === MemberCategory.Alumni && (
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {member.graduationYear || 'N/A'}
                    </td>
                  )}
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <Link
                      href={`/admin/members/${member.id}/edit`}
                      className="text-blue-600 hover:text-blue-900 mr-4"
                    >
                      Edit
                    </Link>
                    <Link
                      href={category === MemberCategory.Staff
                        ? `/staff-hall-of-fame/${generateSlug(member.name)}`
                        : `/alumni-hall-of-fame/${generateSlug(member.name)}`
                      }
                      className="text-green-600 hover:text-green-900"
                    >
                      View
                    </Link>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}