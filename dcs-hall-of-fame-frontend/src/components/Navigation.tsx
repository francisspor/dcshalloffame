'use client'

import Link from 'next/link'
import { useState } from 'react'
import { useMembers } from '@/hooks/useMembers'
import { MemberCategory } from '@/types/member'
import { generateSlug } from '@/utils/slug'

export default function Navigation() {
  const [staffOpen, setStaffOpen] = useState(false)
  const [alumniOpen, setAlumniOpen] = useState(false)
  const { getMembersByCategory, loading } = useMembers()

  const staffMembers = getMembersByCategory(MemberCategory.Staff)
  const alumniMembers = getMembersByCategory(MemberCategory.Alumni)

  return (
    <nav className="bg-blue-800 text-white shadow-lg">
      <div className="container mx-auto px-4">
        <div className="flex items-center justify-between h-16">
          {/* Logo/Home */}
          <Link href="/" className="text-xl font-bold hover:text-blue-200 transition-colors">
            DCS Hall of Fame
          </Link>

          {/* Navigation Items */}
          <div className="hidden md:flex items-center space-x-8">
            <Link href="/" className="hover:text-blue-200 transition-colors">
              Home
            </Link>

            <Link href="/awards" className="hover:text-blue-200 transition-colors">
              Awards
            </Link>

            {/* Staff Hall of Fame Dropdown */}
            <div className="relative group">
              <button
                className="hover:text-blue-200 transition-colors flex items-center"
                onClick={() => setStaffOpen(!staffOpen)}
              >
                Staff Hall of Fame
                <svg className="ml-1 w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                </svg>
              </button>

              {staffOpen && (
                <div className="absolute top-full left-0 mt-1 w-64 bg-white text-gray-800 rounded-md shadow-lg z-50 max-h-96 overflow-y-auto">
                  {loading ? (
                    <div className="px-4 py-2 text-gray-500">Loading...</div>
                  ) : staffMembers.length === 0 ? (
                    <div className="px-4 py-2 text-gray-500">No staff members found</div>
                  ) : (
                    staffMembers.map((member) => (
                      <Link
                        key={member.id}
                        href={`/staff-hall-of-fame/${generateSlug(member.name)}`}
                        className="block px-4 py-2 hover:bg-blue-50 transition-colors"
                        onClick={() => setStaffOpen(false)}
                      >
                        {member.name}
                      </Link>
                    ))
                  )}
                </div>
              )}
            </div>

            {/* Alumni Hall of Fame Dropdown */}
            <div className="relative group">
              <button
                className="hover:text-blue-200 transition-colors flex items-center"
                onClick={() => setAlumniOpen(!alumniOpen)}
              >
                Alumni Hall of Fame
                <svg className="ml-1 w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                </svg>
              </button>

              {alumniOpen && (
                <div className="absolute top-full left-0 mt-1 w-64 bg-white text-gray-800 rounded-md shadow-lg z-50 max-h-96 overflow-y-auto">
                  {loading ? (
                    <div className="px-4 py-2 text-gray-500">Loading...</div>
                  ) : alumniMembers.length === 0 ? (
                    <div className="px-4 py-2 text-gray-500">No alumni members found</div>
                  ) : (
                    alumniMembers.map((member) => (
                      <Link
                        key={member.id}
                        href={`/alumni-hall-of-fame/${generateSlug(member.name)}`}
                        className="block px-4 py-2 hover:bg-blue-50 transition-colors"
                        onClick={() => setAlumniOpen(false)}
                      >
                        {member.name}
                      </Link>
                    ))
                  )}
                </div>
              )}
            </div>
          </div>

          {/* Mobile Menu Button */}
          <button className="md:hidden p-2">
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
            </svg>
          </button>
        </div>
      </div>
    </nav>
  )
}