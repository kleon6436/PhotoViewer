/**
 * @file	RawImageController.cpp
 * @author	kleon6436
 */

#include "pch.h"
#include "RawImageController.h"
#include <memory>        // std::make_unique
#include <vector>        // std::vector
#include <cstdint>       // std::uint8_t
#include <cstring>       // std::memcpy
#include <stdexcept>     // std::runtime_error
#include <algorithm>     // std::max
#include <opencv2/opencv.hpp> // cv::Mat, cv::imdecode, cv::resize, etc.
#include <libraw/libraw.h>    // LibRaw本体

namespace Kchary::ImageController::RawImageControl
{
    bool RawImageController::GetImageData(const wchar_t* path, const ImageReadSettings& imageReadSettings, ImageData& imageData)
    {
        const auto rawProcessor = std::make_unique<LibRaw>();

        auto recycle = [&]()
            {
                if (rawProcessor) rawProcessor->recycle();
            };

        try
        {
            if (rawProcessor->open_file(path) != LIBRAW_SUCCESS)
            {
                throw std::runtime_error("open_file failed");
            }

            if (imageReadSettings.isThumbnailMode)
            {
                if (rawProcessor->unpack_thumb() != LIBRAW_SUCCESS)
                {
                    throw std::runtime_error("unpack_thumb failed");
                }

                auto* thumbnail = rawProcessor->dcraw_make_mem_thumb();
                if (!thumbnail || thumbnail->type != LIBRAW_IMAGE_JPEG)
                {
                    throw std::runtime_error("invalid thumbnail");
                }

                // スマートポインタで自動解放
                std::unique_ptr<libraw_processed_image_t, decltype(&LibRaw::dcraw_clear_mem)> thumbPtr(thumbnail, LibRaw::dcraw_clear_mem);

                cv::Mat buf(1, thumbnail->data_size, CV_8UC1, thumbnail->data);
                auto img = cv::imdecode(buf, GetImreadMode(rawProcessor->imgdata.thumbnail, imageReadSettings.resizeLongSideLength));
                if (img.empty())
                {
                    throw std::runtime_error("thumbnail decode failed");
                }

                const int longSideLength = (std::max)(img.cols, img.rows);
                if (longSideLength > imageReadSettings.resizeLongSideLength)
                {
                    const double ratio = static_cast<double>(imageReadSettings.resizeLongSideLength) / longSideLength;
                    cv::resize(img, img, cv::Size(), ratio, ratio, cv::INTER_AREA);
                }

                m_image = img;
            }
            else
            {
                if (rawProcessor->unpack() != LIBRAW_SUCCESS)
                {
                    throw std::runtime_error("unpack failed");
                }

                if (rawProcessor->dcraw_process() != LIBRAW_SUCCESS)
                {
                    throw std::runtime_error("dcraw_process failed");
                }

                auto* image = rawProcessor->dcraw_make_mem_image();
                if (!image || image->type != LIBRAW_IMAGE_BITMAP || image->colors != 3)
                {
                    throw std::runtime_error("invalid raw image");
                }

                std::unique_ptr<libraw_processed_image_t, decltype(&LibRaw::dcraw_clear_mem)> imagePtr(image, LibRaw::dcraw_clear_mem);

                cv::Mat buf(1, image->data_size, CV_8UC1, image->data);
                m_image = cv::imdecode(buf, cv::ImreadModes::IMREAD_COLOR);
                if (m_image.empty())
                {
                    throw std::runtime_error("raw decode failed");
                }
            }

            const auto dataSize = m_image.total() * m_image.elemSize();
            imageData.buffer.resize(dataSize);
            memcpy(imageData.buffer.data(), m_image.data, dataSize);

            imageData.size = static_cast<unsigned int>(dataSize);
            imageData.stride = static_cast<int>(m_image.step);
            imageData.width = m_image.cols;
            imageData.height = m_image.rows;
        }
        catch (const std::exception& e)
        {
            std::cerr << "RawImageController::GetImageData error: " << e.what() << std::endl;
            recycle();
            return false;
        }

        recycle();
        return true;
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