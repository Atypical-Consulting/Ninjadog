# Homebrew Distribution for Ninjadog

This directory contains the Homebrew formula template and setup instructions for distributing Ninjadog via Homebrew.

## Setup (one-time)

### 1. Create the tap repository

Create a new GitHub repository named `homebrew-tap` under the `Atypical-Consulting` organization:

```bash
gh repo create Atypical-Consulting/homebrew-tap --public \
  --description "Homebrew tap for Atypical Consulting tools"
```

Initialize it with a README and Formula directory:

```bash
git clone https://github.com/Atypical-Consulting/homebrew-tap.git
cd homebrew-tap
mkdir Formula
cat > README.md << 'EOF'
# Atypical Consulting Homebrew Tap

## Installation

```bash
brew tap atypical-consulting/tap
brew install ninjadog
```
EOF
git add .
git commit -m "Initial tap setup"
git push
```

### 2. Create a Personal Access Token

Create a fine-grained PAT with write access to the `homebrew-tap` repository:

1. Go to GitHub Settings > Developer settings > Personal access tokens > Fine-grained tokens
2. Create a token with:
   - **Repository access:** Only `Atypical-Consulting/homebrew-tap`
   - **Permissions:** Contents (Read and write)
3. Add the token as a repository secret named `HOMEBREW_TAP_TOKEN` in the `Ninjadog` repository:
   ```bash
   gh secret set HOMEBREW_TAP_TOKEN --repo Atypical-Consulting/Ninjadog
   ```

### 3. That's it!

The release workflow in `.github/workflows/release.yml` automatically:
1. Builds self-contained binaries for macOS (arm64, x64) and Linux (arm64, x64)
2. Publishes them as GitHub Release assets
3. Computes SHA256 checksums
4. Updates the Homebrew formula in `Atypical-Consulting/homebrew-tap`

## How users install

```bash
# Add the tap (once)
brew tap atypical-consulting/tap

# Install
brew install ninjadog

# Upgrade
brew upgrade ninjadog
```

## Manual formula update

If you need to manually update the formula (e.g., for a hotfix), use the template in `formula-template.rb` and replace the version and SHA256 values.
