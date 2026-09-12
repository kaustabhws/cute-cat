# Azure image generation

User-provided configuration, 10 September 2026:

- Base URL: `https://YOUR-RESOURCE.openai.azure.com/openai/v1`
- Model/deployment: `gpt-image-2`
- API-key environment variable: `AZURE_OPENAI_API_KEY` (process, user, or machine environment)

**Never write the key value into this repository, prompts, logs, screenshots, or the desktop application.** This endpoint is used only by art-production tooling. The delivered app loads bundled artwork and makes no image-generation calls.

Use the bundled Codex imagegen CLI at `%USERPROFILE%\.codex\skills\.system\imagegen\scripts\image_gen.py`. The OpenAI SDK honors `OPENAI_BASE_URL`; map the Azure key into `OPENAI_API_KEY` only in the generation process, without printing it. Restore/remove those process variables afterward. Do not modify the bundled CLI.

The requests were tested successfully with the bundled Python runtime and an OpenAI SDK installed under `.tools/python`. For that local setup, set process `PYTHONPATH` to the absolute `.tools/python` directory before invoking the CLI. Alternatively use a dedicated virtual environment with `openai` and `pillow` installed.

```powershell
$taskPython = "$env:USERPROFILE\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe"
$taskImageCli = "$env:USERPROFILE\.codex\skills\.system\imagegen\scripts\image_gen.py"
$taskOldKey = $env:OPENAI_API_KEY
$taskOldBase = $env:OPENAI_BASE_URL
try {
    $taskImageKey = $env:AZURE_OPENAI_API_KEY
    if (-not $taskImageKey) { $taskImageKey = [Environment]::GetEnvironmentVariable('AZURE_OPENAI_API_KEY', 'User') }
    if (-not $taskImageKey) { $taskImageKey = [Environment]::GetEnvironmentVariable('AZURE_OPENAI_API_KEY', 'Machine') }
    if (-not $taskImageKey) { throw 'AZURE_OPENAI_API_KEY is missing.' }
    $env:OPENAI_API_KEY = $taskImageKey
    $env:OPENAI_BASE_URL = 'https://YOUR-RESOURCE.openai.azure.com/openai/v1'
    & $taskPython $taskImageCli generate --model gpt-image-2 --quality high --size 1024x1024 --prompt-file art-source/prompts/idle.txt --out output/imagegen/cat-idle-source.png --no-augment
} finally {
    $env:OPENAI_API_KEY = $taskOldKey
    $env:OPENAI_BASE_URL = $taskOldBase
    $taskImageKey = $null
}
```

For an identity-preserving edit, use `edit --image <approved-image>` with a new pose prompt and output filename. Do not pass `input_fidelity` for this model. Keep the user-selected model even if newer models appear in generic documentation.

The imagegen CLI reference documents that `gpt-image-2` does not accept `background=transparent`. Generate against a flat saturated chroma-key color, then use the bundled `remove_chroma_key.py` and inspect the alpha locally. Do not silently substitute another model. API errors must be summarized without printing request headers or environment contents.

For the current cream/taupe character, use an explicit balanced `#FF00FF` key in the cleanup command. Auto-sampling an uneven magenta background made the helper misclassify warm fur as red spill during the first cleanup attempt; the final assets were regenerated from the raw outputs with the corrected key and visually checked on light/dark backgrounds. `scripts/prepare_art.py` preserves this fix and rejects implausible alpha distributions.

Official API reference consulted: [Image generation](https://developers.openai.com/api/docs/guides/image-generation), opened 10 September 2026. The current guide emphasizes newer models; Azure availability for this exact deployment is established only by actual requests to the user-provided endpoint.
