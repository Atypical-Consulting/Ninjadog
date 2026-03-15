class Ninjadog < Formula
  desc "CLI tool that generates CRUD Web API boilerplate from a config file"
  homepage "https://github.com/Atypical-Consulting/Ninjadog"
  version "{{VERSION}}"
  license "Apache-2.0"

  on_macos do
    if Hardware::CPU.arm?
      url "https://github.com/Atypical-Consulting/Ninjadog/releases/download/v{{VERSION}}/ninjadog-{{VERSION}}-osx-arm64.tar.gz"
      sha256 "{{SHA256_OSX_ARM64}}"
    else
      url "https://github.com/Atypical-Consulting/Ninjadog/releases/download/v{{VERSION}}/ninjadog-{{VERSION}}-osx-x64.tar.gz"
      sha256 "{{SHA256_OSX_X64}}"
    end
  end

  on_linux do
    if Hardware::CPU.arm?
      url "https://github.com/Atypical-Consulting/Ninjadog/releases/download/v{{VERSION}}/ninjadog-{{VERSION}}-linux-arm64.tar.gz"
      sha256 "{{SHA256_LINUX_ARM64}}"
    else
      url "https://github.com/Atypical-Consulting/Ninjadog/releases/download/v{{VERSION}}/ninjadog-{{VERSION}}-linux-x64.tar.gz"
      sha256 "{{SHA256_LINUX_X64}}"
    end
  end

  def install
    bin.install "ninjadog"
  end

  test do
    assert_match version.to_s, shell_output("#{bin}/ninjadog --version")
  end
end
