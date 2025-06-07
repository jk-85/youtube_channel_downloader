#!/bin/bash

echo "Usage: $0 <youtube_channel_handle> [-nodownload]"
echo "If -nodownload is given, don't download and only create the gallery with files already present"

# Check arguments
if [ -z "$1" ]; then
    echo "Usage: $0 <youtube_channel_handle> [-nodownload]"
    exit 1
fi

CHANNEL_NAME="$1"
SKIP_DOWNLOAD=false
DOWNLOAD_DIR="./$CHANNEL_NAME"

# Check for optional second argument
if [ "$2" == "-nodownload" ]; then
    SKIP_DOWNLOAD=true
fi

# Create folder if not exists
mkdir -p "$DOWNLOAD_DIR"

# Download only if not skipped
if [ "$SKIP_DOWNLOAD" = false ]; then
    echo "Downloading videos into $DOWNLOAD_DIR..."
    yt-dlp \
        --write-description \
        --write-info-json \
        --write-thumbnail \
        --embed-thumbnail \
        --merge-output-format mkv \
        -P "$DOWNLOAD_DIR" \
        "https://www.youtube.com/@$CHANNEL_NAME"
else
    echo "Skipping download. Using existing files in $DOWNLOAD_DIR..."
fi

cd "$DOWNLOAD_DIR" || exit 1

HTML_FILE="index.html"
echo "<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>${CHANNEL_NAME} Youtube Video Gallery</title>
    <style>
        table { width: 100%; border-collapse: collapse; }
        td { padding: 10px; border: 1px solid #ccc; vertical-align: top; width: 16.66%; }
        img { width: 220px; height: auto; display: block; }
        .title { font-weight: bold; }
        .description { font-size: 0.9em; color: #555; word-wrap: break-word; }
        a { color: blue; word-break: break-word; }
    </style>
</head>
<body>
<h1>${CHANNEL_NAME} Youtube Video Gallery</h1>
<table>" > "$HTML_FILE"

# Function to escape only the '#' character
escape_hash() {
    echo "$1" | sed 's/#/%23/g'
}

# Gather and sort JSON files by upload_date (descending)
mapfile -t sorted_jsons < <(
    for f in *.info.json; do
        upload_date=$(jq -r '.upload_date' "$f" 2>/dev/null)
        echo "$upload_date|$f"
    done | sort -r | cut -d'|' -f2
)

entry_index=0
row_index=0

for json in "${sorted_jsons[@]}"; do
    #if (( entry_index == 1 )); then
    #    ((entry_index++))
    #    continue  # Omit the 2nd video
    #fi

    base="${json%.info.json}"
    video_file=$(ls "$base".* 2>/dev/null | grep -E '\.mkv$|\.mp4$|\.webm$' | head -n 1)
    thumb_file=$(ls "$base".* | grep -E '\.jpg$|\.jpeg$|\.png$|\.webp$' | head -n 1)

    [ -f "$video_file" ] && [ -f "$thumb_file" ] || { ((entry_index++)); continue; }

    title=$(jq -r '.title' "$json")

    # Extract and format description (first 3 lines), wrapping every 36 characters
    raw_description=$(jq -r '.description' "$json" | head -n 50000 | tr '\n' ' ')
	# Convert URLs to clickable links with target="_blank"
	linked_description=$(echo "$raw_description" | sed -E 's#(https?://[^ ]+)#<a href="\1" target="_blank">\1</a>#g')
	# Wrap every 36 characters and replace line breaks
	description=$(echo "$linked_description" | fold -s -w 36)

    # Escape '#' character only
    safe_video_file=$(escape_hash "$video_file")
    safe_thumb_file=$(escape_hash "$thumb_file")

    if (( row_index % 6 == 0 )); then
        echo "<tr>" >> "$HTML_FILE"
    fi

    echo "<td>
            <a href=\"$safe_video_file\">
                <img src=\"$safe_thumb_file\" alt=\"$title\">
            </a>
            <div class=\"title\">$title</div>
            <div class=\"description\">$description</div>
          </td>" >> "$HTML_FILE"

    ((entry_index++))
    ((row_index++))

    if (( row_index % 6 == 0 )); then
        echo "</tr>" >> "$HTML_FILE"
    fi
done

if (( row_index % 6 != 0 )); then
    echo "</tr>" >> "$HTML_FILE"
fi

echo "</table>
</body>
</html>" >> "$HTML_FILE"

echo "Gallery generated: $(realpath "$HTML_FILE")"
