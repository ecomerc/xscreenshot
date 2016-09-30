module Fastlane
  module Actions
    class XscreenshotFlAction < Action
      def self.run(params)
        UI.message("The xscreenshot_fl plugin is working!")
      end

      def self.description
        "A reincarnation of the screenshot gem for fastlane that works for Xamarin.Forms apps"
      end

      def self.authors
        ["Peter"]
      end

      def self.available_options
        [
          # FastlaneCore::ConfigItem.new(key: :your_option,
          #                         env_name: "XSCREENSHOT_FL_YOUR_OPTION",
          #                      description: "A description of your option",
          #                         optional: false,
          #                             type: String)
        ]
      end

      def self.is_supported?(platform)
        # Adjust this if your plugin only works for a particular platform (iOS vs. Android, for example)
        # See: https://github.com/fastlane/fastlane/blob/master/fastlane/docs/Platforms.md
        #
        # [:ios, :mac, :android].include?(platform)
        true
      end
    end
  end
end
