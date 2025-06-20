export default function Awards() {
  const awards = [
    {
      name: 'Distinguished Service Award',
      description: 'Recognizes exceptional contributions to the school and community over an extended period.',
      criteria: [
        'Minimum 20 years of service to DCS or community',
        'Demonstrated leadership and dedication',
        'Positive impact on students and community',
        'Exemplary character and values'
      ],
      recipients: ['ROBERT SHAFER', 'CHARLES GUYDER', 'MURIEL CHATTERTON']
    },
    {
      name: 'Excellence in Education Award',
      description: 'Honors outstanding achievements in the field of education and teaching.',
      criteria: [
        'Significant contributions to education',
        'Innovation in teaching methods',
        'Mentorship of students and colleagues',
        'Recognition in educational community'
      ],
      recipients: ['BEATRICE ECKLER RASK', 'RIT MORENO', 'LOIS CHATTERTON-ROGERS-WATSON']
    },
    {
      name: 'Community Leadership Award',
      description: 'Celebrates individuals who have made significant contributions to their communities.',
      criteria: [
        'Leadership in community organizations',
        'Volunteer service and philanthropy',
        'Positive community impact',
        'Role model for others'
      ],
      recipients: ['RUTH EASTON', 'WILLIAM BARNES', 'RICHARD MURRAY']
    },
    {
      name: 'Professional Achievement Award',
      description: 'Recognizes outstanding success in professional careers and industries.',
      criteria: [
        'Exceptional career achievements',
        'Industry recognition and awards',
        'Innovation and leadership in field',
        'Positive representation of DCS values'
      ],
      recipients: ['STEPHEN J. DUBNER', 'NICK GWIAZDOWSKI', 'AMY CHRISTMAN']
    },
    {
      name: 'Lifetime Achievement Award',
      description: 'The highest honor recognizing a lifetime of exceptional contributions and service.',
      criteria: [
        'Lifetime of outstanding achievement',
        'Multiple areas of contribution',
        'Enduring positive impact',
        'Exemplary character throughout life'
      ],
      recipients: ['HOWARD "CAPPY" SCHWORM', 'MILDRED SCHWORM', 'JOE BENA']
    }
  ]

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-yellow-600 text-white shadow-lg">
        <div className="container mx-auto px-4 py-8">
          <h1 className="text-4xl font-bold text-center">
            Awards & Recognition
          </h1>
          <p className="text-xl text-center mt-2">
            Honoring excellence and achievement in our Hall of Fame members
          </p>
        </div>
      </header>

      {/* Main Content */}
      <main className="container mx-auto px-4 py-8">
        {/* Introduction */}
        <section className="mb-12">
          <div className="bg-white rounded-lg shadow-md p-8">
            <h2 className="text-2xl font-bold text-gray-800 mb-4">
              Hall of Fame Awards
            </h2>
            <p className="text-lg text-gray-600 leading-relaxed">
              The DCS Hall of Fame recognizes outstanding achievement through various awards
              that celebrate different aspects of excellence. These awards honor individuals
              who have made significant contributions to education, community service,
              professional achievement, and leadership.
            </p>
          </div>
        </section>

        {/* Awards Grid */}
        <section className="space-y-8">
          {awards.map((award, index) => (
            <div key={index} className="bg-white rounded-lg shadow-md p-8">
              <div className="flex items-start justify-between mb-6">
                <div>
                  <h3 className="text-2xl font-bold text-gray-800 mb-2">
                    {award.name}
                  </h3>
                  <p className="text-lg text-gray-600">
                    {award.description}
                  </p>
                </div>
                <div className="bg-yellow-100 text-yellow-800 px-4 py-2 rounded-full text-sm font-semibold">
                  {award.recipients.length} Recipients
                </div>
              </div>

              <div className="grid md:grid-cols-2 gap-8">
                {/* Criteria */}
                <div>
                  <h4 className="text-lg font-semibold text-gray-800 mb-3">
                    Award Criteria
                  </h4>
                  <ul className="space-y-2">
                    {award.criteria.map((criterion, idx) => (
                      <li key={idx} className="flex items-start">
                        <span className="text-yellow-600 mr-2 mt-1">•</span>
                        <span className="text-gray-600">{criterion}</span>
                      </li>
                    ))}
                  </ul>
                </div>

                {/* Recipients */}
                <div>
                  <h4 className="text-lg font-semibold text-gray-800 mb-3">
                    Notable Recipients
                  </h4>
                  <div className="space-y-2">
                    {award.recipients.map((recipient, idx) => (
                      <div key={idx} className="bg-gray-50 px-4 py-2 rounded-md">
                        <span className="text-gray-800 font-medium">{recipient}</span>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          ))}
        </section>

        {/* Nomination Information */}
        <section className="mt-12">
          <div className="bg-white rounded-lg shadow-md p-8">
            <h3 className="text-2xl font-bold text-gray-800 mb-4">
              Nomination Process
            </h3>
            <div className="grid md:grid-cols-2 gap-8">
              <div>
                <h4 className="text-lg font-semibold text-gray-800 mb-3">
                  How to Nominate
                </h4>
                <ol className="space-y-2 text-gray-600">
                  <li className="flex items-start">
                    <span className="text-yellow-600 mr-2 mt-1">1.</span>
                    <span>Complete the nomination form with detailed information about the candidate</span>
                  </li>
                  <li className="flex items-start">
                    <span className="text-yellow-600 mr-2 mt-1">2.</span>
                    <span>Provide supporting documentation and references</span>
                  </li>
                  <li className="flex items-start">
                    <span className="text-yellow-600 mr-2 mt-1">3.</span>
                    <span>Submit nomination by the annual deadline</span>
                  </li>
                  <li className="flex items-start">
                    <span className="text-yellow-600 mr-2 mt-1">4.</span>
                    <span>Selection committee reviews and evaluates nominations</span>
                  </li>
                </ol>
              </div>
              <div>
                <h4 className="text-lg font-semibold text-gray-800 mb-3">
                  Selection Criteria
                </h4>
                <ul className="space-y-2 text-gray-600">
                  <li className="flex items-start">
                    <span className="text-yellow-600 mr-2 mt-1">•</span>
                    <span>Demonstrated excellence in their field or community</span>
                  </li>
                  <li className="flex items-start">
                    <span className="text-yellow-600 mr-2 mt-1">•</span>
                    <span>Positive representation of DCS values and character</span>
                  </li>
                  <li className="flex items-start">
                    <span className="text-yellow-600 mr-2 mt-1">•</span>
                    <span>Sustained impact and contributions over time</span>
                  </li>
                  <li className="flex items-start">
                    <span className="text-yellow-600 mr-2 mt-1">•</span>
                    <span>Recognition by peers and community</span>
                  </li>
                </ul>
              </div>
            </div>
          </div>
        </section>
      </main>
    </div>
  )
}