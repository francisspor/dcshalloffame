'use client'

import { useState, useEffect } from 'react'
import { MemberCategory, HallOfFameMember } from '@/types/member'

interface MemberFormData {
  name: string
  category: MemberCategory
  graduationYear: string
  biography: string
  imageUrl: string
  achievements: string[]
}

interface MemberFormProps {
  initialData?: HallOfFameMember
  onSubmit: (data: Omit<HallOfFameMember, 'id' | 'createdAt' | 'updatedAt'>) => Promise<void>
  onCancel: () => void
  loading?: boolean
  error?: string
  submitLabel: string
  title: string
  subtitle: string
}

export default function MemberForm({
  initialData,
  onSubmit,
  onCancel,
  loading = false,
  error,
  submitLabel,
  title,
  subtitle
}: MemberFormProps) {
  const [formData, setFormData] = useState<MemberFormData>({
    name: '',
    category: MemberCategory.Staff,
    graduationYear: '',
    biography: '',
    imageUrl: '',
    achievements: ['']
  })

  // Initialize form with existing data if editing
  useEffect(() => {
    if (initialData) {
      setFormData({
        name: initialData.name,
        category: initialData.category,
        graduationYear: initialData.graduationYear?.toString() || '',
        biography: initialData.biography,
        imageUrl: initialData.imageUrl,
        achievements: initialData.achievements.length > 0 ? initialData.achievements : ['']
      })
    }
  }, [initialData])

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target
    setFormData(prev => ({
      ...prev,
      [name]: value
    }))
  }

  const handleAchievementChange = (index: number, value: string) => {
    const newAchievements = [...formData.achievements]
    newAchievements[index] = value
    setFormData(prev => ({
      ...prev,
      achievements: newAchievements
    }))
  }

  const addAchievement = () => {
    setFormData(prev => ({
      ...prev,
      achievements: [...prev.achievements, '']
    }))
  }

  const removeAchievement = (index: number) => {
    if (formData.achievements.length > 1) {
      const newAchievements = formData.achievements.filter((_, i) => i !== index)
      setFormData(prev => ({
        ...prev,
        achievements: newAchievements
      }))
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    // Filter out empty achievements
    const filteredAchievements = formData.achievements.filter(achievement => achievement.trim() !== '')

    const memberData: Omit<HallOfFameMember, 'id' | 'createdAt' | 'updatedAt'> = {
      name: formData.name.trim(),
      category: formData.category,
      graduationYear: formData.graduationYear ? parseInt(formData.graduationYear) : undefined,
      biography: formData.biography.trim(),
      imageUrl: formData.imageUrl.trim(),
      achievements: filteredAchievements
    }

    await onSubmit(memberData)
  }

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">{title}</h1>
        <p className="text-gray-600">{subtitle}</p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4">
            <p className="text-red-600">{error}</p>
          </div>
        )}

        {/* Basic Information */}
        <div className="grid md:grid-cols-2 gap-6">
          <div>
            <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-2">
              Full Name *
            </label>
            <input
              type="text"
              id="name"
              name="name"
              value={formData.name}
              onChange={handleInputChange}
              required
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent text-gray-900 placeholder-gray-500"
              placeholder="Enter full name"
            />
          </div>

          <div>
            <label htmlFor="category" className="block text-sm font-medium text-gray-700 mb-2">
              Category *
            </label>
            <select
              id="category"
              name="category"
              value={formData.category}
              onChange={handleInputChange}
              required
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent text-gray-900"
            >
              <option value={MemberCategory.Staff}>Staff</option>
              <option value={MemberCategory.Alumni}>Alumni</option>
            </select>
          </div>
        </div>

        {/* Graduation Year */}
        <div>
          <label htmlFor="graduationYear" className="block text-sm font-medium text-gray-700 mb-2">
            Graduation Year
          </label>
          <input
            type="number"
            id="graduationYear"
            name="graduationYear"
            value={formData.graduationYear}
            onChange={handleInputChange}
            min="1900"
            max={new Date().getFullYear() + 1}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent text-gray-900 placeholder-gray-500"
            placeholder="Leave empty for staff members"
          />
        </div>

        {/* Image URL */}
        <div>
          <label htmlFor="imageUrl" className="block text-sm font-medium text-gray-700 mb-2">
            Image URL
          </label>
          <input
            type="url"
            id="imageUrl"
            name="imageUrl"
            value={formData.imageUrl}
            onChange={handleInputChange}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent text-gray-900 placeholder-gray-500"
            placeholder="https://example.com/image.jpg"
          />
        </div>

        {/* Biography */}
        <div>
          <label htmlFor="biography" className="block text-sm font-medium text-gray-700 mb-2">
            Biography *
          </label>
          <textarea
            id="biography"
            name="biography"
            value={formData.biography}
            onChange={handleInputChange}
            required
            rows={6}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent text-gray-900 placeholder-gray-500"
            placeholder="Enter the member's biography..."
          />
        </div>

        {/* Achievements */}
        <div>
          <div className="flex items-center justify-between mb-4">
            <label className="block text-sm font-medium text-gray-700">
              Achievements
            </label>
            <button
              type="button"
              onClick={addAchievement}
              className="px-3 py-1 text-sm bg-green-600 text-white rounded hover:bg-green-700 transition-colors"
            >
              Add Achievement
            </button>
          </div>
          <div className="space-y-3">
            {formData.achievements.map((achievement, index) => (
              <div key={index} className="flex items-center space-x-3">
                <input
                  type="text"
                  value={achievement}
                  onChange={(e) => handleAchievementChange(index, e.target.value)}
                  className="flex-1 px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent text-gray-900 placeholder-gray-500"
                  placeholder="Enter achievement..."
                />
                {formData.achievements.length > 1 && (
                  <button
                    type="button"
                    onClick={() => removeAchievement(index)}
                    className="px-3 py-2 text-red-600 hover:text-red-800 transition-colors"
                  >
                    Remove
                  </button>
                )}
              </div>
            ))}
          </div>
        </div>

        {/* Form Actions */}
        <div className="flex items-center justify-end space-x-4 pt-6 border-t">
          <button
            type="button"
            onClick={onCancel}
            className="px-6 py-2 border border-gray-300 text-gray-700 rounded-md hover:bg-gray-50 transition-colors"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={loading}
            className="px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {loading ? 'Saving...' : submitLabel}
          </button>
        </div>
      </form>
    </div>
  )
}