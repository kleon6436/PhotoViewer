#include "pch.h"
/**
 * @file	RawImageController.cpp
 * @author	kleon6436
 */

#include <memory>
#include <libraw/libraw.h>
#include "RawImageController.h"

namespace Kchary::ImageController::RawImageControl
{
	int RawImageController::GetImageData(const char* path, ImageData& imageData) const
	{
		const auto rawProcessor = std::make_unique<LibRaw>();

		// Read raw image using libraw.
		if (rawProcessor->open_file(path) != LIBRAW_SUCCESS)
		{
			// Todo: Error sequence.
			return -1;
		}

		// Unpack the image.
		if (rawProcessor->unpack() != LIBRAW_SUCCESS)
		{
			// Todo: Error sequence.
			return -1;
		}

		if (rawProcessor->dcraw_process() != LIBRAW_SUCCESS)
		{
			// Todo: Error sequence.
			return -1;
		}

		auto* const image = rawProcessor->dcraw_make_mem_image();
		if (!image || image->type != LIBRAW_IMAGE_BITMAP || image->colors != 3)
		{
			// Todo: Error sequence.
			return -1;
		}

		if (image->bits == 16 || image->bits == 8)
		{
			const auto dataSize = image->data_size;
			const auto* const rawData = static_cast<std::uint8_t*>(image->data);

			imageData.buffer = new std::uint8_t[dataSize];
			memcpy(imageData.buffer, rawData, dataSize);

			imageData.size = dataSize;
			imageData.stride = static_cast<int>(image->width * image->colors * image->bits / 8);
			imageData.width = static_cast<int>(image->width);
			imageData.height = static_cast<int>(image->height);
		}
		else
		{
			//Todo: Error sequence.
			return -1;
		}

		// Free all internal data.
		LibRaw::dcraw_clear_mem(image);
		rawProcessor->recycle();

		return 0;
	}

	int RawImageController::GetThumbnailImageData(const char* path, const int resizeLongSideLength, ImageData& imageData) const
	{
		const auto rawProcessor = std::make_unique<LibRaw>();

		// Read raw image using libraw.
		if (rawProcessor->open_file(path) != LIBRAW_SUCCESS)
		{
			// Todo: Error sequence.
			return -1;
		}

		if (rawProcessor->unpack_thumb() != LIBRAW_SUCCESS)
		{
			// Todo: Error sequence.
			return -1;
		}

		// Get thumbnail struct data.
		auto* thumbnail = rawProcessor->dcraw_make_mem_thumb();
		if (!thumbnail || thumbnail->type != LibRaw_image_formats::LIBRAW_IMAGE_JPEG)
		{
			// Todo: Error sequence.
			return -1;
		}

		// Read thumbnail jpeg image data using opencv.
		// (Raw image's thumbnail data is jpeg.)
		const auto thumbnailDataSize = thumbnail->data_size;
		auto* const thumbnailData = static_cast<std::uint8_t*>(thumbnail->data);

		const auto imreadMode = GetImreadMode(rawProcessor->imgdata.thumbnail, resizeLongSideLength);
		const std::vector<std::uint8_t> jpegData(thumbnailData, thumbnailData + thumbnailDataSize);
		auto img = cv::imdecode(jpegData, imreadMode);

		// Resize image data.
		const auto longSideLength = img.cols > img.rows ? img.cols : img.rows;
		const auto ratio = (static_cast<double>(resizeLongSideLength) / static_cast<double>(longSideLength));
		cv::Mat resizeImg;
		cv::resize(img, resizeImg, cv::Size(), ratio, ratio, cv::INTER_AREA);

		const auto imgDataSize = resizeImg.total() * resizeImg.elemSize();
		imageData.buffer = new std::uint8_t[imgDataSize];
		memcpy(imageData.buffer, resizeImg.data, imgDataSize * sizeof(std::uint8_t));

		// Translate data to C#
		imageData.size = static_cast<unsigned int>(imgDataSize);
		imageData.stride = static_cast<int>(resizeImg.step);
		imageData.width = resizeImg.cols;
		imageData.height = resizeImg.rows;

		img.release();
		resizeImg.release();

		// Free all internal data.
		LibRaw::dcraw_clear_mem(thumbnail);
		rawProcessor->recycle();

		return 0;
	}

	cv::ImreadModes RawImageController::GetImreadMode(const libraw_thumbnail_t& thumbnail, const int resizeLongSideLength)
	{
		const auto thumbLongSideLength = thumbnail.twidth > thumbnail.theight ? thumbnail.twidth : thumbnail.theight;

		cv::ImreadModes imreadMode;

		if (resizeLongSideLength <= thumbLongSideLength / 8)
		{
			imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_8;
		}
		else if (thumbLongSideLength / 8 < resizeLongSideLength && resizeLongSideLength <= thumbLongSideLength / 4)
		{
			imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_4;
		}
		else if (thumbLongSideLength / 4 < resizeLongSideLength && resizeLongSideLength <= thumbLongSideLength / 2)
		{
			imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_2;
		}
		else
		{
			imreadMode = cv::ImreadModes::IMREAD_COLOR;
		}

		return imreadMode;
	}
}