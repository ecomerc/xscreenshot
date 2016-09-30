# coding: utf-8
lib = File.expand_path("../lib", __FILE__)
$LOAD_PATH.unshift(lib) unless $LOAD_PATH.include?(lib)
require 'fastlane/plugin/xscreenshot_fl/version'

Gem::Specification.new do |spec|
  spec.name          = 'fastlane-plugin-xscreenshot_fl'
  spec.version       = Fastlane::XscreenshotFl::VERSION
  spec.author        = %q{Peter}
  spec.email         = %q{peter@ecomerc.com}

  spec.summary       = %q{A reincarnation of the screenshot gem for fastlane that works for Xamarin.Forms apps}
  # spec.homepage      = "https://github.com/<GITHUB_USERNAME>/fastlane-plugin-xscreenshot_fl"
  spec.license       = "MIT"

  spec.files         = Dir["lib/**/*"] + %w(README.md LICENSE)
  spec.test_files    = spec.files.grep(%r{^(test|spec|features)/})
  spec.require_paths = ['lib']

  # spec.add_dependency 'your-dependency', '~> 1.0.0'

  spec.add_development_dependency 'pry'
  spec.add_development_dependency 'bundler'
  spec.add_development_dependency 'rspec'
  spec.add_development_dependency 'rake'
  spec.add_development_dependency 'rubocop'
  spec.add_development_dependency 'fastlane', '>= 1.95.0'
end
