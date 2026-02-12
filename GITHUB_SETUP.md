# Step-by-Step: Add GitHub URL and Push Your Code

## Prerequisites
- Git installed on your machine
- A GitHub account
- A repository on GitHub (create one at https://github.com/new if you haven’t)

---

## Step 1: Create a repository on GitHub (if you don’t have one)

1. Go to **https://github.com/new**
2. Enter a **Repository name** (e.g. `poultry-distribution-system`)
3. Choose **Public** or **Private**
4. **Do not** initialize with README, .gitignore, or license (you already have them locally)
5. Click **Create repository**
6. Copy the repository URL. It will look like:
   - **HTTPS:** `https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git`
   - **SSH:** `git@github.com:YOUR_USERNAME/YOUR_REPO_NAME.git`

---

## Step 2: Add the GitHub URL as “origin”

Open a terminal in your project folder (`d:\cursor checks` or your project root) and run:

```powershell
git remote add origin https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git
```

Replace `YOUR_USERNAME` and `YOUR_REPO_NAME` with your actual GitHub username and repo name.

**Examples:**
- `git remote add origin https://github.com/johndoe/poultry-distribution-system.git`
- Or with SSH: `git remote add origin git@github.com:johndoe/poultry-distribution-system.git`

**Check that it was added:**
```powershell
git remote -v
```
You should see `origin` with your URL for fetch and push.

---

## Step 3: Stage your files

Stage everything (respecting `.gitignore`):

```powershell
git add .
```

Or stage specific items:
```powershell
git add .
git status
```
Review the list; only tracked (non-ignored) files will be committed.

---

## Step 4: Commit

```powershell
git commit -m "Initial commit: Poultry Distribution System"
```

Use any message you like. If you already have commits, use a message that describes your latest changes.

---

## Step 5: Push to GitHub

**If this is the first push** and your branch is `main`:

```powershell
git branch -M main
git push -u origin main
```

**If your branch is `master`:**
```powershell
git branch -M main
git push -u origin main
```
(This renames the branch to `main` and pushes it.)

**Later pushes** (after `-u` is set):
```powershell
git push
```

---

## Step 6: Verify on GitHub

1. Open your repo in the browser: `https://github.com/YOUR_USERNAME/YOUR_REPO_NAME`
2. Confirm that your folders (e.g. `frontend`, `PoultryDistributionSystem.API`) and files are there.

---

## Quick reference

| Task | Command |
|------|--------|
| Add remote | `git remote add origin <URL>` |
| See remotes | `git remote -v` |
| Change remote URL | `git remote set-url origin <NEW_URL>` |
| Stage all | `git add .` |
| Commit | `git commit -m "Your message"` |
| First push | `git push -u origin main` |
| Later pushes | `git push` |

---

## If you get errors

**“remote origin already exists”**  
You already added a remote. To replace it:
```powershell
git remote set-url origin https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git
```

**“Authentication failed” / “Permission denied”**  
- For HTTPS: use a **Personal Access Token** instead of your password (GitHub → Settings → Developer settings → Personal access tokens).  
- For SSH: set up an SSH key and add it to your GitHub account.

**“Failed to push some refs”**  
If the GitHub repo has content (e.g. README) that you don’t have locally:
```powershell
git pull origin main --rebase
git push -u origin main
```
Or, if you’re sure you want to overwrite the remote:
```powershell
git push -u origin main --force
```
Use `--force` only when you intend to replace the remote history.
