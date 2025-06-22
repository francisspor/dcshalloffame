'use client'

import { useState, useEffect, useCallback } from 'react'
import { HallOfFameMember, MemberCategory } from '@/types/member'
import { apiService } from '@/services/api'
import { findMemberBySlug } from '@/utils/slug'

export function useMembers() {
  const [members, setMembers] = useState<HallOfFameMember[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchMembers()
  }, [])

  const fetchMembers = async () => {
    setLoading(true)
    setError(null)

    const result = await apiService.getAllMembers()

    if (result.error) {
      setError(result.error)
    } else if (result.data) {
      setMembers(result.data)
    }

    setLoading(false)
  }

  const getMembersByCategory = (category: MemberCategory) => {
    return members.filter(member => member.category === category)
  }

  const getMemberById = (id: string) => {
    return members.find(member => member.id === id)
  }

  const getMemberBySlug = (slug: string) => {
    return findMemberBySlug(members, slug)
  }

  const refreshMembers = () => {
    fetchMembers()
  }

  return {
    members,
    loading,
    error,
    getMembersByCategory,
    getMemberById,
    getMemberBySlug,
    refreshMembers
  }
}

export function useMemberById(id: string) {
  const [member, setMember] = useState<HallOfFameMember | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchMember = useCallback(async () => {
    setLoading(true)
    setError(null)

    const result = await apiService.getMemberById(id)

    if (result.error) {
      setError(result.error)
    } else if (result.data) {
      setMember(result.data)
    }

    setLoading(false)
  }, [id])

  useEffect(() => {
    if (id) {
      fetchMember()
    }
  }, [id, fetchMember])

  return {
    member,
    loading,
    error,
    refreshMember: fetchMember
  }
}

export function useMemberBySlug(slug: string) {
  const [member, setMember] = useState<HallOfFameMember | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchMember = useCallback(async () => {
    setLoading(true)
    setError(null)

    const result = await apiService.getMemberBySlug(slug)

    if (result.error) {
      setError(result.error)
    } else if (result.data) {
      setMember(result.data)
    }

    setLoading(false)
  }, [slug])

  useEffect(() => {
    if (slug) {
      fetchMember()
    }
  }, [slug, fetchMember])

  return {
    member,
    loading,
    error,
    refreshMember: fetchMember
  }
}

export function useStatistics() {
  const [statistics, setStatistics] = useState<{
    totalMembers: number;
    staffMembers: number;
    alumniMembers: number;
    categories: { [key: string]: number };
  } | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchStatistics()
  }, [])

  const fetchStatistics = async () => {
    setLoading(true)
    setError(null)

    const result = await apiService.getStatistics()

    if (result.error) {
      setError(result.error)
    } else if (result.data) {
      setStatistics(result.data)
    }

    setLoading(false)
  }

  return {
    statistics,
    loading,
    error,
    refreshStatistics: fetchStatistics
  }
}