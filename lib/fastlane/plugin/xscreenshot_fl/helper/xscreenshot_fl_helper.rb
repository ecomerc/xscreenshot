module Fastlane
  module Helper
    class XscreenshotFlHelper
      # class methods that you define here become available in your action
      # as `Helper::XscreenshotFlHelper.your_method`
      #
      def self.show_message
        UI.message("Hello from the xscreenshot_fl plugin helper!")
      end
    end
  end
end
