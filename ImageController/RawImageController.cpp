#include "pch.h"
/**
 * @file	RawImageController.cpp
 * @author	kchary6436
 */

#include <memory>
#include <libraw/libraw.h>
#include "RawImageController.h"

namespace Kchary::ImageController::RawImageControl
{
	int RawImageController::GetImageData(const char* path, ImageData* imageData) const
	{
		const std::unique_ptr<LibRaw> rawProcessor = std::make_unique<LibRaw>();

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

		const auto image = rawProcessor->dcraw_make_mem_image();
		if (!image || image->type != LIBRAW_IMAGE_BITMAP || image->colors != 3)
		{
			// Todo: Error sequence.
			return -1;
		}

		if (image->bits == 16 || image->bits == 8)
		{
			const auto dataSize = image->data_size;
			const auto rawData = (std::uint8_t*)image->data;

			imageData->buffer = new std::uint8_t[dataSize];
			memcpy(imageData->buffer, rawData, dataSize);

			imageData->size = dataSize;
			imageData->stride = image->width * image->colors * image->bits / 8;
			imageData->width = image->width;
			imageData->height = image->height;
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

	int RawImageController::GetThumbnailImageData(const char* path, int resizeLongSideLength, ImageData* imageData) const
	{
		const std::unique_ptr<LibRaw> rawProcessor = std::make_unique<LibRaw>();

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
		libraw_processed_image_t* thumbnail = rawProcessor->dcraw_make_mem_thumb();
		if (!thumbnail || thumbnail->type != LibRaw_thumbnail_formats::LIBRAW_THUMBNAIL_JPEG)
		{
			// Todo: Error sequence.
			return -1;
		}

		// Read thumbnail jpeg image data using opencv.
		// (Raw image's thumbnail data is jpeg.)
		const auto thumbnailDataSize = thumbnail->data_size;
		const auto thumbnailData = (std::uint8_t*)thumbnail->data;

		const auto imreadMode = GetImreadMode(rawProcessor->imgdata.thumbnail, resizeLongSideLength);
		const std::vector<std::uint8_t> jpegData(thumbnailData, thumbnailData + thumbnailDataSize);
		cv::Mat img = cv::imdecode(jpegData, imreadMode);

		// Resize image data.
		int longSideLength = img.cols > img.rows ? img.cols : img.rows;
		double ratio = ((double)resizeLongSideLength / (double)longSideLength);
		cv::Mat resizeImg;
		cv::resize(img, resizeImg, cv::Size(), ratio, ratio, cv::INTER_AREA);

		const auto imgDataSize = resizeImg.total() * resizeImg.elemSize();
		imageData->buffer = new std::uint8_t[imgDataSize];
		memcpy(imageData->buffer, resizeImg.data, imgDataSize * sizeof(std::uint8_t));

		// Translate data to C#
		imageData->size = (unsigned int)imgDataSize;
		imageData->stride = (int)resizeImg.step;
		imageData->width = resizeImg.cols;
		imageData->height = resizeImg.rows;

		img.release();
		resizeImg.release();

		// Free all internal data.
		rawProcessor->dcraw_clear_mem(thumbnail);
		rawProcessor->recycle();

		return 0;
	}

	cv::ImreadModes RawImageController::GetImreadMode(libraw_thumbnail_t thumbnail, int resizeLongSideLength) const
	{
		const ushort thumbLongSideLength = thumbnail.twidth > thumbnail.theight ? thumbnail.twidth : thumbnail.theight;

		auto imreadMode = cv::ImreadModes::IMREAD_COLOR;
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