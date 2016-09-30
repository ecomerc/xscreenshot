describe Fastlane::Actions::XscreenshotFlAction do
  describe '#run' do
    it 'prints a message' do
      expect(Fastlane::UI).to receive(:message).with("The xscreenshot_fl plugin is working!")

      Fastlane::Actions::XscreenshotFlAction.run(nil)
    end
  end
end
