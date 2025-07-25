import Link from 'next/link'
import { apiService } from '@/services/api'

interface MemberPageProps {
  params: Promise<{
    slug: string
  }>
}

export default async function AlumniMemberPage({ params }: MemberPageProps) {
  const { slug } = await params

  // Fetch member data server-side
  const result = await apiService.getMemberBySlug(slug)
  const member = result.data

  if (!member) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="bg-red-50 border border-red-200 rounded-lg p-8 max-w-md">
            <p className="text-red-600 text-lg mb-4">
              Member not found
            </p>
            <Link
              href="/alumni-hall-of-fame"
              className="px-6 py-2 bg-purple-600 text-white rounded hover:bg-purple-700 transition-colors"
            >
              Back to Alumni Hall of Fame
            </Link>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-purple-700 text-white shadow-lg">
        <div className="container mx-auto px-4 py-8">
          <div className="flex items-center justify-between">
            <div>
              <Link href="/alumni-hall-of-fame" className="text-purple-200 hover:text-white transition-colors">
                ← Back to Alumni Hall of Fame
              </Link>
            </div>
          </div>
          <div className="text-center mt-4">
            <h1 className="text-4xl font-bold">
              {member.name}
            </h1>
            <p className="text-xl mt-2">
              {member.graduationYear && `Class of ${member.graduationYear}`}
            </p>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="container mx-auto px-4 py-8">
        <div className="grid lg:grid-cols-3 gap-8">
          {/* Main Content */}
          <div className="lg:col-span-2 space-y-8">
            {/* Biography */}
            {member.biography && (
              <section className="bg-white rounded-lg shadow-md p-8">
                <h2 className="text-2xl font-bold text-gray-800 mb-4">
                  Biography
                </h2>
                <div className="prose prose-lg text-gray-600">
                  <p>{member.biography}</p>
                </div>
              </section>
            )}

            {/* Achievements */}
            {member.achievements && member.achievements.length > 0 && (
              <section className="bg-white rounded-lg shadow-md p-8">
                <h2 className="text-2xl font-bold text-gray-800 mb-4">
                  Notable Achievements
                </h2>
                <div className="space-y-4">
                  {member.achievements.map((achievement, index) => (
                    <div key={index} className="border-l-4 border-purple-500 pl-4">
                      <p className="text-gray-600">{achievement}</p>
                    </div>
                  ))}
                </div>
              </section>
            )}
          </div>

          {/* Sidebar */}
          <div className="space-y-6">
            {/* Member Image */}
            {member.imageUrl && (
              <div className="bg-white rounded-lg shadow-md p-6 flex flex-col items-center">
                <img
                  src={member.imageUrl}
                  alt={member.name}
                  className="w-40 h-40 object-cover rounded-full border-4 border-purple-300 shadow-md mb-2"
                  loading="lazy"
                />
              </div>
            )}
            {/* Quick Info */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <h3 className="text-lg font-bold text-gray-800 mb-4">
                Quick Information
              </h3>
              <div className="space-y-3">
                {member.graduationYear && (
                  <div>
                    <span className="text-gray-500 text-sm">Graduation Year:</span>
                    <p className="font-medium">{member.graduationYear}</p>
                  </div>
                )}
                <div>
                  <span className="text-gray-500 text-sm">Category:</span>
                  <p className="font-medium">Alumni</p>
                </div>
              </div>
            </div>

            {/* Share */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <h3 className="text-lg font-bold text-gray-800 mb-4">
                Share This Profile
              </h3>
              <div className="space-y-2">
                <button className="w-full bg-blue-600 text-white py-2 px-4 rounded hover:bg-blue-700 transition-colors">
                  Share on Facebook
                </button>
                <button className="w-full bg-blue-400 text-white py-2 px-4 rounded hover:bg-blue-500 transition-colors">
                  Share on Twitter
                </button>
                <button className="w-full bg-gray-600 text-white py-2 px-4 rounded hover:bg-gray-700 transition-colors">
                  Copy Link
                </button>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  )
}